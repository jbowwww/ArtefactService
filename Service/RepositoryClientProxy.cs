using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TextWriter=System.IO.TextWriter;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using Serialize.Linq.Nodes;
using System.Reflection;
using System.Diagnostics;
using Serialize.Linq.Extensions;
using System.Collections;
using NHibernate.Linq;

namespace Artefacts.Service
{
	/// <summary>
	/// Repository client proxy.
	/// </summary>
	/// <remarks>
	/// 
	/// 	
	///		public int QueryCount(object queryId)
	///		{
	///			return _channel.QueryCount(queryId);
	///		}
	///
	///		public TArtefact QueryResult(object queryId)
	///		{
	///			return _channel.QueryResult(queryId);
	///		}
	///
	/// 
	/// </remarks>
	[ServiceKnownType("GetArtefactTypes", typeof(Artefact))]
	public class RepositoryClientProxy : IArtefactService, IOrderedQueryable<Artefact>, IQueryProvider, IDisposable
	{
		#region Private fields
		private bool _init;
		private bool _close;
		private readonly string _proxyTypeName;
		#endregion
		
		#region Service channel
		/// <summary>
		/// The binding.
		/// </summary>
		public readonly Binding Binding;

		/// <summary>
		/// The address.
		/// </summary>
		public readonly string Address;

		/// <summary>
		/// The channel factory.
		/// </summary>
		public ChannelFactory<IArtefactService> ChannelFactory;

		/// <summary>
		/// The channel.
		/// </summary>
		public IArtefactService Channel;
		#endregion
		
		#region Queries		
		/// <summary>
		/// An expression visitor used in constructor
		/// </summary>
		public ExpressionVisitor ExpressionVisitor;

		/// <summary>
		/// Gets the default paging options.
		/// </summary>
		public PagingOptions DefaultPagingOptions { get; set; }
		
		/// <summary>
		/// The _query cache.
		/// </summary>
		public Dictionary<object, IQueryable> QueryCache;

		/// <summary>
		/// Gets or sets the queryables.
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IDictionary<Type, IQueryable> Queryables { get; private set; }
		
		/// <summary>
		/// Gets or sets the artefacts.
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IQueryable<Artefact> Artefacts { get; private set; }
		
		/// <summary>
		/// Gets or sets the artefacts.
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IQueryable<Host> Hosts { get; private set; }
		#endregion

		#region IQueryable implementation
		public Type ElementType {
			get { return typeof(Artefact); }
		}

		public Expression Expression {
			get { return _expression; }
		}
		private readonly Expression _expression = Expression.Parameter(typeof(IArtefactService), "Repository");

		public IQueryProvider Provider {
			get { return this; }
		}
		#endregion
		
		#region Construction & disposal methods
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.RepositoryClientProxy"/> class.
		/// </summary>
		/// <param name="binding">Binding.</param>
		/// <param name="address">Address.</param>
		/// <param name="visitor">Client side <see cref="ExpressionVisitor"/> </param>
		public RepositoryClientProxy(Binding binding, string address, bool init = true)
		{
			_init = _close = false;
			_proxyTypeName = GetType().FullName;
			if (Repository.Context != null)
				throw new ApplicationException("Repository.Context != null");
			Repository.Context = this;
			Binding = binding;
			Address = address;
			if (init)
				Init();
		}
		
		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			if (_init)
				Console.WriteLine("{0}: Already initialised", _proxyTypeName);
			else
			{
				_init = true;
				Console.WriteLine("{0}: Initialising", _proxyTypeName);
				
				ChannelFactory = new ChannelFactory<IArtefactService>(Binding, Address);				
	//			ApplyChannelFactoryBehaviours(_channelFactory);
				Channel = ChannelFactory.CreateChannel();
				
				Queryables = new ConcurrentDictionary<Type, IQueryable>();
				QueryCache = new Dictionary<object, IQueryable>();
				ExpressionVisitor = new ClientQueryVisitor(this, QueryCache);
				Artefacts = BuildBaseQuery<Artefact>();
				Hosts = BuildBaseQuery<Host>();
				
				DefaultPagingOptions = new PagingOptions();
			}
		}
		
		/// <summary>
		/// Builds the base query.
		/// </summary>
		/// <returns>The base query.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public IQueryable<T> BuildBaseQuery<T>()
		{
			Type _TArtefact = typeof(T);
			return (IQueryable<T>)(Queryables[_TArtefact] ??
				(Queryables[_TArtefact] = ((IQueryProvider)this).CreateQuery<T>(
				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { _TArtefact },
					Expression.Property(null, typeof(Repository).GetProperty("Session", BindingFlags.Static | BindingFlags.Public))))));
		}

		/// <summary>
		/// Applies the channel factory behaviours.
		/// </summary>
		/// <param name="factory">Factory.</param>
		private void ApplyChannelFactoryBehaviours(ChannelFactory<IArtefactService> factory)
		{
//			foreach (OperationDescription operation in factory.Endpoint.Contract.Operations)
//			{
//				DataContractSerializerOperationBehavior dcsb = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
//				if (dcsb == null)
//					operation.Behaviors.Add(dcsb = new MyDataContractBehaviour(operation));
//				dcsb.DataContractResolver = new WCFTypeResolver();
//				dcsb.DataContractSurrogate = new WCFDataSerializerSurrogate();
//			}
		}


		/// <summary>
		/// Releases all resource used by the <see cref="Artefacts.Service.RepositoryClientProxy"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Artefacts.Service.RepositoryClientProxy"/>.
		/// The <see cref="Dispose"/> method leaves the <see cref="Artefacts.Service.RepositoryClientProxy"/> in an unusable
		/// state. After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Artefacts.Service.RepositoryClientProxy"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Artefacts.Service.RepositoryClientProxy"/> was occupying.</remarks>
		public void Dispose()
		{
			Close();
		}
		
		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Close()
		{
			if (_close)
				Console.WriteLine("{0}: Already closed", _proxyTypeName);
			else
			{
				_close = true;
//				Channel.Disconnect(Host.Current);
				ChannelFactory.Close();
			}
		}
		#endregion

		#region IQueryProvider implementation
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression</param>
		/// <returns>The query</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		public IQueryable CreateQuery(Expression expression)
		{
			return (IQueryable)((IQueryProvider)this).CreateQuery<Artefact>(expression);
		}

		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TElement">The 1st type parameter.</typeparam>
		/// <returns>The query.</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (!expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "Should implement System.Collections.IEnumerable");
				
			Expression parsedExpression = ExpressionVisitor.Visit(expression);
			object id = parsedExpression.Id();
			IQueryable query;
			if (QueryCache.TryGetValue(id, out query))
				return (IQueryable<TElement>)query;
			QueryCache[id] = (IQueryable)typeof(Queryable<>).MakeGenericType(typeof(TElement))
				.GetConstructor(new Type[] { typeof(RepositoryClientProxy), typeof(Expression) })
				.Invoke(new object[] { this, parsedExpression });
			return (IQueryable<TElement>)QueryCache[id];
		}

		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>The query result</returns>
		/// <remarks>
		/// IQueryProvider implementation
		/// - Trying to handle FirstOrDefault etc methods here
		/// // This only doesn't work because my queyr provider executes it using a repository query method (QueryExecute)
		/// that has return type of object, and should only be used for scalar results. (It does not have any artefact KnownType's)
		/// for method calls like FirstOrDefault() that produce an Artefact, you
		/// will need to find some way of detecting that return value, and running the method call expression's argument[0] expression as a query, to
		/// get artefact id, then retrieve artefact using repository getbyid() ??
		/// </remarks>
		public object Execute(Expression expression)
		{
			Expression parsedExpression = ExpressionVisitor.Visit(expression);
			return Channel.QueryExecute(parsedExpression.ToBinary());
		}
//			if (typeof(Artefact).IsAssignableFrom(expression.Type) && expression.IsMethodCallExpression())
//			{
//				MethodCallExpression mce = (MethodCallExpression)expression;
//				Expression instanceExp = mce.Object ?? (mce.Arguments.Count > 0 ? mce.Arguments[0] : null);
//				if (instanceExp != null && instanceExp.IsQueryable())
//				{
//					int[] r = Channel.QueryResults(Channel.QueryPreload(ExpressionVisitor.Visit(instanceExp).ToBinary()));
//					string methodName = mce.Method.Name;
//					if (methodName.Equals("First"))
//					{
//						if (r.Length == 0)
//							throw new IndexOutOfRangeException("First() method callled on empty array");
//						return Channel.GetById(r[0]);
//					}
//					else if (methodName.Equals("FirstOrDefault"))
//						return r.Length == 0 ? null : Channel.GetById(r[0]);
//					if (methodName.Equals("Last"))
//					{
//						if (r.Length == 0)
//							throw new IndexOutOfRangeException("Last() method callled on empty array");
//						return Channel.GetById(r[r.Length - 1]);
//					}
//					else if (methodName.Equals("LastOrDefault"))
//						return r.Length == 0 ? null : Channel.GetById(r[r.Length - 1]);
//					else
//						throw new NotSupportedException("Method \"" + methodName + "\" not supported");
//				}
//			}
//			return !typeof(Artefact).IsAssignableFrom(parsedExpression.Type) ||
//				parsedExpression.NodeType != ExpressionType.Constant ?
//					Channel.QueryExecute(parsedExpression.ToBinary())
//					: ((ConstantExpression)parsedExpression).Value;		//.ToExpressionNode());	


				/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		/// <returns>The query result</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		TResult IQueryProvider.Execute<TResult>(Expression expression)
		{
			return (TResult)Execute(expression);
		}
		#endregion
		
		#region IEnumerable implementation		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		/// <remarks><see cref="System.Collections.Generic.IEnumerable[Artefact]" /> implementation</remarks>
		public IEnumerator<Artefact> GetEnumerator()
		{
			return Artefacts.GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <remarks>IEnumerable implementation</remarks>
		System.Collections.IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}
		#endregion
	}
}

