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
	public class RepositoryClientProxy : IRepository, IOrderedQueryable<Artefact>, IQueryProvider, IDisposable
	{
		#region Fields & properties
		private bool _init;
		private bool _close;
		private readonly string _proxyTypeName;
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
		public ChannelFactory<IRepository> ChannelFactory;

		/// <summary>
		/// The channel.
		/// </summary>
		public IRepository Channel;
		
		private int _clientId;
		#endregion
		
		#region Queries
		/// <summary>
		/// The _query cache.
		/// </summary>
		public Dictionary<object, IQueryable> QueryCache;

		/// <summary>
		/// An expression visitor used in constructor
		/// </summary>
		public ExpressionVisitor ExpressionVisitor;

		/// <summary>
		/// Gets the default paging options.
		/// </summary>
		public PagingOptions DefaultPagingOptions { get; set; }
		#endregion

		#region Collections/Enumerables/Queryables
		/// <summary>
		/// Gets or sets the artefacts.
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IQueryable<Artefact> Artefacts { get; private set;}		//{ get { return Channel.Artefacts; } }

		/// <summary>
		/// Gets or sets the queryables.
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IDictionary<Type, IQueryable> Queryables { get; private set; }
		#endregion

		#region IQueryable implementation
		public Type ElementType {
			get { return typeof(Artefact); }
		}

		public Expression Expression {
			get { return _expression; }
		}
		private readonly Expression _expression = Expression.Parameter(typeof(IRepository), "Repository");

		public IQueryProvider Provider {
			get { return this; }
		}
		#endregion
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
				Console.WriteLine("{0}: Initialising", _proxyTypeName);
				ChannelFactory = new ChannelFactory<IRepository>(Binding, Address);				
	//			ApplyChannelFactoryBehaviours(_channelFactory);
				Channel = ChannelFactory.CreateChannel();
				Queryables = new ConcurrentDictionary<Type, IQueryable>();
				QueryCache = new Dictionary<object, IQueryable>();
				DefaultPagingOptions = new PagingOptions();
				ExpressionVisitor = new ClientQueryVisitor(this, QueryCache);
				Artefacts = BuildBaseQuery<Artefact>();
				// ((IQueryProvider)this).CreateQuery<Artefact>(Expression.Property(Expression, "Artefacts"));
				//(IQueryable<Artefact>)this;		//
				//			Queryables.Add(typeof(Artefact), Artefacts);
				Host.Current = BuildBaseQuery<Host>().Where(host => Host.GetHostId() == host.HostId).FirstOrDefault() ?? new Host();
				_clientId = Channel.Connect(Host.Current);
				if (Host.Current.IsTransient)
					Channel.Add(Host.Current);
				else
					Channel.Update(Host.Current.Update());
				
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
				Channel.Disconnect(Host.Current);
				ChannelFactory.Close();
			}
		}
		
		/// <summary>
		/// Builds the base query.
		/// </summary>
		/// <returns>The base query.</returns>
		/// <typeparam name="TArtefact">The 1st type parameter.</typeparam>
		public IQueryable<TArtefact> BuildBaseQuery<TArtefact>() where TArtefact : Artefact
		{
//			IQueryable<TArtefact> query = Expression.PropertyOrField()
				var g6= ((IQueryProvider)this).CreateQuery<TArtefact>(
				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(TArtefact) },
					Expression.Property(null, typeof(Repository).GetProperty("Session", BindingFlags.Static | BindingFlags.Public))));
//				Expression.Call(
//					typeof(NHibernate.Linq.LinqExtensionMethods).GetMethods()
//						.First((mi) => mi.Name.Equals("Query") && mi.GetGenericArguments().Length == 1)
//							.MakeGenericMethod(typeof(TArtefact)), Expression.Property(Expression.Constant(null),
//						typeof(Repository).GetProperty("Session", BindingFlags.Static | BindingFlags.Public))));
					//"Query", new Type[] { typeof(TArtefact) }));
			Queryables[typeof(TArtefact)] = g6;
			return g6;
//			return Artefacts.OfType<TArtefact>();
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
		
		public int Connect(Host host)
		{
			throw new NotImplementedException();
		}
		
		public void Disconnect(Host host)
		{
			throw new NotImplementedException();
		}
		
		#region Add/Get/Update/Remove singular artefact operations
		/// <summary>
		/// Add the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public int Add(Artefact artefact)
		{
			return (artefact.Id = Channel.Add(artefact)).Value;
		}

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public int GetId(Artefact artefact)
		{
			return Channel.GetId(artefact);
		}

		/// <summary>
		/// Gets the by identifier.
		/// </summary>
		/// <returns>The by identifier.</returns>
		/// <param name="id">Identifier.</param>
		/// <remarks>IRepository implementation</remarks>
		public Artefact GetById(int id)
		{
			return Channel.GetById(id);
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
				Channel.Update(artefact);
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
			Channel.Remove(artefact);
		}
		#endregion
		
		#region Query methods
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="expression">Expression.</param>
		/// <remarks>IRepository implementation</remarks>
		public object QueryPreload(byte[] expression)	//ExpressionNode expression)
		{
			return Channel.QueryPreload(expression);
		}
	
		/// <summary>
		/// Queries the results.
		/// </summary>
		/// <returns>The results.</returns>
		/// <param name="queryId">Query identifier.</param>
		/// <param name="startIndex">Start index.</param>
		/// <param name="count">Count.</param>
		public int[] QueryResults(object queryId, int startIndex = 0, int count = -1)
		{
			return Channel.QueryResults(queryId, startIndex, count);
		}

		/// <summary>
		/// Queries the execute.
		/// </summary>
		/// <returns>The execute.</returns>
		/// <param name="expression">Expression.</param>
		public object QueryExecute(byte[] expression)		// ExpressionNode expression)
		{
			return Channel.QueryExecute(expression);		//binary);
		}
		#endregion

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

		#region IQueryProvider implementation
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression</param>
		/// <returns>The query</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		public IQueryable CreateQuery(Expression expression)
		{
//			Type T = expression.GetElementType();
//			MethodInfo mi = GetType().GetMethod("CreateQuery",
//				                BindingFlags.Instance | BindingFlags.NonPublic);
//				.MakeGenericMethod(T);

//			Debug.Assert(mi != null);// && mi.IsGenericMethod);
//			return (IQueryable)mi.Invoke(this, new object[] { expression });

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

			// TODO
			// 31.5.14
			// Should send expression to server here, take return identifier (that is baed on something simple and cheap to calculate)
			// and supply it to constructor/property init'er of Queryable. That way when Queryable.*Enumerator*
			// is used it can just supply the cheap simple identifier to the server operation in exchange for results (or maybe a
			// similar method of returning id's only, .. .. dunno .. )

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
//			if (expression.IsEnumerable() && typeof(TArtefact).IsAssignableFrom(expression.Type) && _queryCache.ContainsKey(expression))
//				return _queryCache[expression];
//			if (expression.IsEnumerable())
//				throw new InvalidOperationException();

			if (typeof(Artefact).IsAssignableFrom(expression.Type) && expression.IsMethodCallExpression())
			{
				MethodCallExpression mce = (MethodCallExpression)expression;
				Expression instanceExp = mce.Object ?? (mce.Arguments.Count > 0 ? mce.Arguments[0] : null);
				if (instanceExp != null && instanceExp.IsQueryable())
				{
					int[] r = Channel.QueryResults(Channel.QueryPreload(ExpressionVisitor.Visit(instanceExp).ToBinary()));
					string methodName = mce.Method.Name;
					if (methodName.Equals("First"))
					{
						if (r.Length == 0)
							throw new IndexOutOfRangeException("First() method callled on empty array");
						return Channel.GetById(r[0]);
					}
					else if (methodName.Equals("FirstOrDefault"))
						return r.Length == 0 ? null : Channel.GetById(r[0]);
					if (methodName.Equals("Last"))
					{
						if (r.Length == 0)
							throw new IndexOutOfRangeException("Last() method callled on empty array");
						return Channel.GetById(r[r.Length - 1]);
					}
					else if (methodName.Equals("LastOrDefault"))
						return r.Length == 0 ? null : Channel.GetById(r[r.Length - 1]);
					else
						throw new NotSupportedException("Method \"" + methodName + "\" not supported");
				}
			}

			Expression parsedExpression = ExpressionVisitor.Visit(expression);
			return !typeof(Artefact).IsAssignableFrom(parsedExpression.Type) ||
				parsedExpression.NodeType != ExpressionType.Constant ?
					Channel.QueryExecute(parsedExpression.ToBinary())
					: ((ConstantExpression)parsedExpression).Value;		//.ToExpressionNode());	
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
		#endregion
	}
}

