using System;
//using TextWriter=System.IO.TextWriter;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Serialize.Linq.Extensions;
using NHibernate.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using Serialize.Linq.Nodes;
using System.Reflection;
using System.Diagnostics;

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
	public class RepositoryClientProxy<TArtefact> : IQueryProvider, IOrderedQueryable<TArtefact>, IDisposable
		where TArtefact : Artefact
	{
		#region Private fields
		private readonly string _proxyTypeName = typeof(RepositoryClientProxy<TArtefact>).FullName;
		private bool _init = false;
		private bool _close = false;
		private ChannelFactory<IRepository> _channelFactory;
		private IRepository _channel;
		private ExpressionVisitor _expressionVisitor;
		private PagingOptions _defaultPagingOptions;
		private Expression _expression;
		private Queryable<TArtefact> _artefacts;
		private Dictionary<object, IQueryable> _queryCache;
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
		/// The channel.
		/// </summary>
		public IRepository Channel {
			get { return _channel; }
		}

		/// <summary>
		/// Gets the default paging options.
		/// </summary>
		public PagingOptions DefaultPagingOptions {
			get { return _defaultPagingOptions; }
		}
		#endregion

		#region IQueryable implementation
		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		public Type ElementType {
			get { return typeof(TArtefact); }
		}

		/// <summary>
		/// Gets the expression.
		/// </summary>
		/// <value>The expression.</value>
		public Expression Expression {
			get { return _expression; }
		}

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
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
			if (Repository.Context != null)
				throw new ApplicationException("Repository.Context != null");
//			Repository.Context = this;

			Binding = binding;
			Address = address;

			if (init)
				Init();
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
		#endregion
		
		#region (De)Initialisation routines
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
				Console.WriteLine("{0}: Initialising\n\tBinding: {1}\n\tAddress: {2}\n", _proxyTypeName, Binding.ToString (), Address.ToString ());
				_channelFactory = new ChannelFactory<IRepository>(Binding, Address);
//				ApplyChannelFactoryBehaviours(_channelFactory);
				_channel = _channelFactory.CreateChannel();
				_defaultPagingOptions = new PagingOptions();
				_expressionVisitor = new ClientQueryVisitor();
				_queryCache = new Dictionary<object, IQueryable>();
				_expression = Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(TArtefact) },
					Expression.Property(null, typeof(Repository).GetProperty("Session", BindingFlags.Static | BindingFlags.Public)));
				_artefacts = (Queryable<TArtefact>)CreateQuery(_expression);
				Console.WriteLine ("{0}: Initialised\n", _proxyTypeName);
			}
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
				Console.WriteLine("{0}: Closing", _proxyTypeName);
				_channel.Disconnect(Host.Current);
				_channelFactory.Close();
				Console.WriteLine ("{0}: Closed\n", _proxyTypeName);
			}
		}

		/// <summary>
		/// Applies the channel factory behaviours.
		/// </summary>
		/// <param name="factory">Factory.</param>
		private void ApplyChannelFactoryBehaviours(ChannelFactory<IRepository> factory)
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
		#endregion

		#region Add/Get/Update/Remove singular artefact operations
		/// <summary>
		/// Add the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public int Add(Artefact artefact)
		{
			return (artefact.Id = _channel.Add(artefact)).Value;
		}

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public int GetId(Artefact artefact)
		{
			return _channel.GetId(artefact);
		}

		/// <summary>
		/// Gets the by identifier.
		/// </summary>
		/// <returns>The by identifier.</returns>
		/// <param name="id">Identifier.</param>
		/// <remarks>IRepository implementation</remarks>
		public Artefact GetById(int id, Type T = null)
		{
			return _channel.GetById(id);
		}

		/// <summary>
		/// Update the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public void Update(Artefact artefact)
		{
			if (artefact.TimeUpdatesCommitted < artefact.TimeUpdated)
			{
				_channel.Update(artefact);
				artefact.TimeUpdatesCommitted = DateTime.Now;
			}
		}

		/// <summary>
		/// Remove the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public void Remove(Artefact artefact)
		{
			_channel.Remove(artefact);
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
		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)// where TElement : Artefact
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (!expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "Should implement System.Collections.IEnumerable");
			if (!typeof(Artefact).IsAssignableFrom(typeof(TElement)))
				throw new ArgumentOutOfRangeException("TElement", typeof(TElement), "Should be subclass of Artefact");

			Expression parsedExpression = _expressionVisitor.Visit(expression);
		object parsedExpressionId = parsedExpression.FormatString(); //parsedExpression.Id();
			if (!_queryCache.ContainsKey(parsedExpressionId))
				_queryCache[parsedExpressionId] =
					(IQueryable)typeof(Queryable<TArtefact>)//.MakeGenericType(typeof(TElement))
					.GetConstructors()[0]
//						.GetConstructor(new Type[] { typeof(RepositoryClientProxy<>), typeof(Expression) })
						.Invoke(new object[] { this, parsedExpression, parsedExpressionId });
			return (IQueryable<TElement>)_queryCache[parsedExpressionId];
		}

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
			return _channel.QueryExecute(_expressionVisitor.Visit(expression).ToBinary());
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		/// <remarks><see cref="System.Collections.Generic.IEnumerable[Artefact]" /> implementation</remarks>
		public IEnumerator<TArtefact> GetEnumerator()
		{
			return _artefacts.GetEnumerator();
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

