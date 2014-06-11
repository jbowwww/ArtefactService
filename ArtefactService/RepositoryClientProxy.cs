using System;
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
	public class RepositoryClientProxy : IRepository, IQueryProvider
	{
		#region Fields & properties
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
		public readonly ChannelFactory<IRepository> ChannelFactory;

		/// <summary>
		/// The channel.
		/// </summary>
		public readonly IRepository Channel;
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
		#endregion
		
		#region Construction & initialisation methods
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.RepositoryClientProxy"/> class.
		/// </summary>
		/// <param name="binding">Binding.</param>
		/// <param name="address">Address.</param>
		/// <param name="visitor">Client side <see cref="ExpressionVisitor"/> </param>
		public RepositoryClientProxy(Binding binding, string address, ExpressionVisitor visitor = null)
		{
			if (Repository.Context != null)
				throw new ApplicationException("Repository.Context != null");
			Repository.Context = this;
			Binding = binding;
			Address = address;
			ChannelFactory = new ChannelFactory<IRepository>(Binding, Address);				
//			ApplyChannelFactoryBehaviours(_channelFactory);
			Channel = ChannelFactory.CreateChannel();

			QueryCache = new Dictionary<object, IQueryable>();
			ExpressionVisitor = visitor ?? new ClientQueryVisitor(this, QueryCache);

			DefaultPagingOptions = new PagingOptions();
			Queryables = new ConcurrentDictionary<Type, IQueryable>();
			Queryables.Add(typeof(Artefact), Artefacts);
			Artefacts = (IQueryable<Artefact>)BuildBaseQuery<Artefact>();

//				IQueryable qu = (IQueryable<Artefact>) CreateQuery(
//				Expression.PropertyOrField(
//					Expression.Variable(typeof(IRepository), "Repository"),
			//typeof(IQueryable<Artefact>)
		}

		public IQueryable<TArtefact> BuildBaseQuery<TArtefact>() where TArtefact : Artefact
		{
//			IQueryable<TArtefact> query = Expression.PropertyOrField()
				var g6= ((IQueryProvider )this).CreateQuery<TArtefact>(
				Expression.Call(typeof(NHibernate.Linq.LinqExtensionMethods).
					GetMethods().First((mi) => mi.Name.Equals("Query") && mi.GetGenericArguments().Length == 1
					).MakeGenericMethod(typeof(TArtefact)), Expression.Property(/*Expression.Constant(*/null,//),
						typeof(Repository).GetProperty("Session", BindingFlags.Static | BindingFlags.Public))));
					//"Query", new Type[] { typeof(TArtefact) }));
			Queryables[typeof(TArtefact)] = g6;
			return g6;
		}

		/// <summary>
		/// Applies the channel factory behaviours.
		/// </summary>
		/// <param name="factory">Factory.</param>
		private void ApplyChannelFactoryBehaviours(ChannelFactory<IRepository> factory)
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
		
		#region Add/Get/Update/Remove singular artefact operations
		/// <summary>
		/// Add the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public int Add(Artefact artefact)
		{
			return Channel.Add(artefact);
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
			Channel.Update(artefact);
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

		#region IQueryProvider implementation
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression</param>
		/// <returns>The query</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		public IQueryable CreateQuery(Expression expression)
		{
			Type T = expression.GetElementType();
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
		/// <remarks>IQueryProvider implementation</remarks>
		public object Execute(Expression expression)
		{
//			if (expression.IsEnumerable() && typeof(TArtefact).IsAssignableFrom(expression.Type) && _queryCache.ContainsKey(expression))
//				return _queryCache[expression];
			if (expression.IsEnumerable())
				throw new InvalidOperationException();
			Expression parsedExpression = ExpressionVisitor.Visit(expression);
			return Channel.QueryExecute(parsedExpression.ToBinary());		//.ToExpressionNode());	
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

