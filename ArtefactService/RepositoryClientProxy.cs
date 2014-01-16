using System;
using System.Collections.Generic;
using TextWriter=System.IO.TextWriter;
//using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

using NHibernate.Linq;

using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

namespace Artefacts.Services
{
	public class RepositoryClientProxy<TArtefact> :
		IRepository<TArtefact>
		where TArtefact : Artefact
	{
		#region Private fields
		private ChannelFactory<IRepository<TArtefact>> _channelFactory = null;
		private IRepository<TArtefact> _channel = null;
		#endregion
		
		#region Properties
		public Binding Binding { get; private set; }
		
		public string Address { get; private set; }
		
		public System.Linq.IQueryProvider QueryProvider { get; private set; }
		
		public System.Linq.IQueryable<TArtefact> Artefacts { get; private set; }
		#endregion
		
		#region Constructors & initialisation methods
		private void ApplyChannelFactorySettings(ChannelFactory<IRepository<TArtefact>> factory)
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
		
		private void Init()
		{
			_channelFactory = new ChannelFactory<IRepository<TArtefact>>(Binding, Address);				
			ApplyChannelFactorySettings(_channelFactory);
			_channel = _channelFactory.CreateChannel();
			
			QueryProvider = new ClientQueryProvider<TArtefact>(_channel);
			Expression expression = 
//				Expression.Empty();
//				Expression.IsTrue(Expression.Constant(true));
				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(TArtefact) },
				Expression.Call(typeof(ArtefactRepository).GetProperty("Session", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
//					)(.Variable(typeof(ArtefactRepository)), "Session"));
			
			Artefacts = QueryProvider.CreateQuery<TArtefact>(expression);
		}
		
		public RepositoryClientProxy(Binding binding, string address)
		{
			Binding = binding;
			Address = address;
			Init();
		}
		#endregion
		
		#region IRepository[TArtefact] implementation
		
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
		
		#region Collections/Enumerables/Queryables
		public object CreateQuery(ExpressionNode expressionNode)
		{
			return _channel.CreateQuery(expressionNode);
		}
		
		public object CreateQuery_EN_Binary(byte[] binary)
		{
			throw new NotImplementedException();
		}
		
		public int QueryCount(object queryId)
		{
			return _channel.QueryCount(queryId);
		}

		public Artefact[] QueryResults(object queryId, int startIndex = 0, int count = -1)
		{
			return _channel.QueryResults(queryId, startIndex, count);
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

