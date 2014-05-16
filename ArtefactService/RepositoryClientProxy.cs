using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TextWriter=System.IO.TextWriter;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Reflection;
using System.Diagnostics;

using NHibernate.Linq;
using Serialize.Linq.Nodes;

namespace Artefacts.Service
{
	public class RepositoryClientProxy<TArtefact> :
		IRepository<TArtefact>
		where TArtefact : Artefact
	{
		#region Fields & properties
		#region Service channel
		public Binding Binding { get; private set; }
		public string Address { get; private set; }
		private ChannelFactory<IRepository<TArtefact>> _channelFactory = null;
		private IRepository<TArtefact> _channel = null;
		#endregion
		
		#region Queries
		public IQueryProvider QueryProvider { get; private set; }
		private Dictionary<object, IQueryable<Artefact>> _queryCache;
		
		/// <summary>
		/// Gets or sets the queryables.
		/// </summary>
		/// <remarks>IRepository[TArtefact] implementation</remarks>
		public IDictionary<Type, IQueryable> Queryables { get; protected set; }
		
		/// <summary>
		/// Gets or sets the artefacts.
		/// </summary>
		/// <remarks>IRepository[TArtefact] implementation</remarks>
		public IQueryable<Artefact> Artefacts { get; private set; }
		#endregion
		#endregion
		
		#region Constructors & initialisation methods
		public RepositoryClientProxy(Binding binding, string address)
		{
			Binding = binding;
			Address = address;
			Init();
		}
		
		private void Init()
		{
			_channelFactory = new ChannelFactory<IRepository<TArtefact>>(Binding, Address);				
//			ApplyChannelFactoryBehaviours(_channelFactory);
			_channel = _channelFactory.CreateChannel();
			
			_queryCache = new Dictionary<object, IQueryable<Artefact>>();
			QueryProvider = new ClientQueryProvider<TArtefact>(this);
			Expression expression = 
				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(Artefact) },
				Expression.Call(typeof(ArtefactRepository).GetProperty("Session", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
			Artefacts = (IQueryable<Artefact>)QueryProvider.CreateQuery(expression);
				
				// experimentation
//				Expression.MakeMemberAccess(
//				Expression.Quote()
//					Expression.Constant(this, typeof(IRepository<TArtefact>)),
//			 		typeof(IRepository<TArtefact>).GetProperty("Artefacts")));			       
			
			Queryables = new ConcurrentDictionary<Type, IQueryable>();
			Queryables.Add(typeof(Artefact), Artefacts);
	
		}

		private void ApplyChannelFactoryBehaviours(ChannelFactory<IRepository<TArtefact>> factory)
		{
			foreach (OperationDescription operation in factory.Endpoint.Contract.Operations)
			{
				DataContractSerializerOperationBehavior dcsb = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
				if (dcsb == null)
					operation.Behaviors.Add(dcsb = new MyDataContractBehaviour(operation));
				dcsb.DataContractResolver = new WCFTypeResolver();
				dcsb.DataContractSurrogate = new WCFDataSerializerSurrogate();
			}
		}
		#endregion
		
		#region IRepository[TArtefact] implementation	
		#region Collections/Enumerables/Queryables
		
		#endregion
		
		#region Add/Get/Update/Remove singular artefact operations
		public int Add(TArtefact artefact)
		{
			return _channel.Add(artefact);
		}

		public int GetId(TArtefact artefact)
		{
			return _channel.GetId(artefact);
		}

		public TArtefact GetById(int id)
		{
			return _channel.GetById(id);
		}

		public void Update(TArtefact artefact)
		{
			_channel.Update(artefact);
		}

		public void Remove(TArtefact artefact)
		{
			_channel.Remove(artefact);
		}
		#endregion
		
		#region Query methods		
		public object CreateQuery(ExpressionNode expression)		//byte[] binary)
		{
			return _channel.CreateQuery(expression);
		}
		
//		public int QueryCount(object queryId)
//		{
//			return _channel.QueryCount(queryId);
//		}
//
//		public TArtefact QueryResult(object queryId)
//		{
//			return _channel.QueryResult(queryId);
//		}
//
		public TArtefact[] QueryResults(object queryId, int startIndex = 0, int count = -1)
		{
			return _channel.QueryResults(queryId, startIndex, count);
		}
//
//		public object QueryMethodCall(object queryId, string methodName)//  MethodInfo method)
//		{
//			return _channel.QueryMethodCall(queryId, methodName);
//		}
		
		public object QueryExecute(byte[] binary)
		{
			return _channel.QueryExecute(binary);
		}
		#endregion
		
		#region Get/Set default paging options
		public PagingOptions GetDefaultPagingOptions()
		{
			return _channel.GetDefaultPagingOptions();
		}

		public void SetDefaultPagingOptions(PagingOptions pagingOptions)
		{
			_channel.SetDefaultPagingOptions(pagingOptions);
		}
		#endregion
		#endregion
	}
}

