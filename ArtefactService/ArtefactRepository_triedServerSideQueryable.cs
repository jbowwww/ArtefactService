using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
//using WCFChannel = System.ServiceModel.Channels;
using System.Reflection;
using System.Diagnostics;

using Serialize.Linq;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

using NHibernate;
using NHibernate.Linq;

namespace Artefacts.Services
{
	/// <summary>
	/// Artefact repository
	/// </summary>
	/// <remarks>
	///	-	Removed parameters of <see cref="ServiceBehaviourAttribute"/>:
	///			MaxItemsInObjectGraph=100,
	///			ReleaseServiceInstanceOnTransactionComplete=false)]
	/// </remarks>
//	[ServiceBehavior(
//		IncludeExceptionDetailInFaults=true,
//		InstanceContextMode=InstanceContextMode.Single,
//		ConcurrencyMode=ConcurrencyMode.Single)]
	[ServiceBehavior(IncludeExceptionDetailInFaults=true,
		InstanceContextMode=InstanceContextMode.Single,
		ConcurrencyMode=ConcurrencyMode.Single)]
	public class ArtefactRepository : IRepository<Artefact>
	{		
		#region Static members
		/// <summary>
		/// Constant artefact update age limit.
		/// </summary>
		/// <remarks>May not remain constant - make configurable by clients</remarks>
		public static TimeSpan ArtefactUpdateAgeLimit = new TimeSpan(0, 1, 0);
		public static ITransaction Transaction { get { return Session.Transaction; } }
		public static ISession Session { get { return NhBootStrap.Session; } }
		#endregion
		
		#region Private fields
		private Dictionary<int, Artefact> _artefactCache;
		private Dictionary<object, IQueryable<Artefact>> _queryCache;
		private Dictionary<object, int> _countCache;
//		private Dictionary<object, QueryResult<Artefact>> _queryResultCache;
		private ConcurrentQueue<object> _queryExecuteQueue;
		private INhQueryProvider _nhQueryProvider;
		private BinaryFormatter _binaryFormatter;
		#endregion

		public ArtefactServiceConfiguration Configuration { get; private set; }
		
		public ArtefactRepository()
		{
			Configuration = new ArtefactServiceConfiguration();

			_artefactCache = new Dictionary<int, Artefact>();
			_queryCache = new Dictionary<object, Queryable<Artefact>>();
			_countCache = new Dictionary<object, int>();
//			_queryResultCache = new Dictionary<object, QueryResult<Artefact>>();
			_queryExecuteQueue = new ConcurrentQueue<object>();
			_nhQueryProvider = new DefaultQueryProvider(Session.GetSessionImplementation());
			_binaryFormatter = new BinaryFormatter();
			_binaryFormatter.Context = new Queryable<Artefact>.QueryableStreamingContext(_nhQueryProvider);
			
			Expression expression = 
//				Expression.Empty();
//				Expression.IsTrue(Expression.Constant(true));
				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(Artefact) },
				Expression.Call(typeof(ArtefactRepository).GetProperty("Session", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
			Artefacts = new Queryable<Artefact>(this, _nhQueryProvider, expression);
//				_nhQueryProvider.CreateQuery<Artefact>(expression);
//			Session.Query<Artefact>();
//				new Queryable<Artefact>(this, _nhQueryProvider, expression);
//				new QueryableNhProxy<Artefact>(new NhQueryable<Artefact>(_nhQueryProvider, Expression.Empty));			//				(NhQueryable<Artefact>)_nhQueryProvider.CreateQuery<Artefact>(Expression.Empty));
			Queryables = new ConcurrentDictionary<Type, IQueryable>(
				new KeyValuePair<Type, IQueryable>[] { new KeyValuePair<Type, IQueryable>(typeof(Artefact), Artefacts)});
		}

		#region IRepository[Artefact] implementation		
		#region Add/Get/Update/Remove singular artefact operations
		public int Add(Artefact artefact)
		{
			ITransaction transaction = null;
			int id = -1;
			try
			{
				if (Session.Transaction == null || !Session.Transaction.IsActive)
					transaction = Session.BeginTransaction();
				artefact.Id = id = (int)Session.Save(artefact);
				_artefactCache.Add(id, artefact);
				if (transaction != null)
					transaction.Commit();
				return id;
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				if (!artefact.IsTransient)
				{
					if (transaction == null)
						Session.Delete(artefact);
					if (_artefactCache.ContainsKey(id))
						_artefactCache.Remove(id);
				}
				throw Error(ex, artefact);
			}
			finally
			{
				if (transaction != null)
					transaction.Dispose();
			}
		}

		public int GetId(Artefact artefact)
		{
			try
			{
				return artefact.IsTransient ? (artefact.Id = (int)Session.GetIdentifier(artefact)).Value : artefact.Id.Value;
			}
			catch (Exception ex)
			{
				throw Error(ex, artefact);
			}
		}

		public Artefact GetById(int id)
		{
			try
			{
				return _artefactCache.ContainsKey(id) && (_artefactCache[id].UpdateAge > ArtefactUpdateAgeLimit)
					? _artefactCache[id] : _artefactCache[id] = Session.Get<Artefact>(id);
			}
			catch (Exception ex)
			{
				throw Error(ex, id);
			}
		}

		public void Update(Artefact artefact)
		{
			ITransaction transaction = null;
			try
			{
				if (Session.Transaction == null || !Session.Transaction.IsActive)
					transaction = Session.BeginTransaction();
//				int id = artefact.Id.Value;
//				if (_artefactCache.ContainsKey(id))
//					_artefactCache[id].CopyMembersFrom(artefact);
//				else
//					_artefactCache[id] = artefact;
//				Session.Update(artefact, artefact.Id);						// TODO: Think about this: if Session.Update fails _artefactCache will be invalid??
				Session.Merge(artefact);						// TODO: Think about this: if Session.Update fails _artefactCache will be invalid??
				if (transaction != null)
					transaction.Commit();
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				throw Error(ex, artefact);
			}
			finally
			{
				if (transaction != null)
					transaction.Dispose();
			}
		}

		public void Remove(Artefact artefact)
		{
			ITransaction transaction = null;
//			int id = -1;
			try
			{
				if (!artefact.IsTransient)
				{
					int id = artefact.Id.Value;
					if (Session.Transaction == null || !Session.Transaction.IsActive)
						transaction = Session.BeginTransaction();
					Session.Delete(artefact.Id);
					if (_artefactCache.ContainsKey(id))
						_artefactCache.Remove(id);
					if (transaction != null)
						transaction.Commit();
				}
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
//				else
//					Session.Save(artefact, id);
				throw Error(ex, artefact);
			}
			finally
			{
				if (transaction != null)
					transaction.Dispose();
			}
		}
		#endregion
		
		#region Query methods		
		/// <summary>
		/// Create a query (serialized using binary formatter)
		/// </summary>
		/// <returns>The query identifier</returns>
		/// <param name="binary">Binary</param>
		/// <remarks>
		/// // TODO: I think I just need to traverse the expression tree from en.ToExpression(),
		/// find the expression node representing the root query from the client side (RepositoryClientProxy.Artefacts)
		/// The expression corresponding to that node is (currently, see source in mentioned class)  method call expression
		/// to ISession.Query<>(). As a 1st parameter (maybe.. I think..?) this will take the ISession paramter. From client side
		/// this is probably null, initialise it to this.Session before creating the query
		/// Do this and this shit might even work?!
		/// 16/1/14: ^^ It appears that this may well be false - may be almost working
		/// 17/1/14: ^^ Not quite, still have trouble with certain things, e.g. using a string variable to compare with
		///							an artefact's member value - tries to serialize the object containing the string instance the
		///							variable references, when I just wanted to use the string value
		///							-	I think one way or another I will have to write code to traverse the expression trees, to prepare them when and as necessary
		/// </remarks>
		public object CreateQuery(byte[] binary)
		{
			try
			{
				BinaryFormatter bf = new BinaryFormatter();	// { Binder = new QuerySerializationBinder() };
				ExpressionNode en = (ExpressionNode)bf.Deserialize(new System.IO.MemoryStream(binary));
				Expression expression = en.ToExpression();
				if (expression.Type.GetInterface("System.Collections.IEnumerable") == null)
				{
					object r = Artefacts.Provider.Execute(expression);
					return r;
				}
				
				object queryId = expression.ToString();
				if (!_queryCache.ContainsKey(queryId))
				{
					Queryable<Artefact> q = (Queryable<Artefact>)
//						new NhQueryable<Artefact>(Artefacts.Provider, expression);
//						Artefacts.Provider.Execute<IQueryable<Artefact>>(expression);
					Artefacts.Provider.CreateQuery<Artefact>(expression);
					_queryCache.Add(queryId, q);					//(Queryable<Artefact>)				// Session.Query<Artefact>().Provider.CreateQuery<Artefact>(en.ToExpression());
				}
				return queryId;
			}
			catch (Exception ex)
			{
				throw Error(ex, binary);
			}
		}
		
		public Queryable<Artefact> CreateQueryable(byte[] binary)
		{
			try
			{
				Expression expression = binary.FromBinary(_binaryFormatter);
				object queryId = expression.Id();
				return _queryCache.ContainsKey(queryId) ?
					_queryCache[queryId] : 
					_queryCache[queryId] = _nhQueryProvider.CreateQuery(expression);
//						new Queryable<Artefact>(this, _nhQueryProvider, expression);
			}
			catch (Exception ex)
			{
				throw Error(ex, binary);
			}
		}
		
		public Queryable<Artefact> GetQueryable(object queryId)
		{
			return _queryCache[queryId];
		}
		
		/// <summary>
		/// Gets number of results in a query
		/// </summary>
		/// <returns>Number of results in the query</returns>
		/// <param name="queryId">Query identifier</param>
		public int QueryCount(object queryId)
		{
			return _countCache.ContainsKey(queryId) ?
				_countCache[queryId] :
				_countCache[queryId] = _queryCache[queryId].Count;
		}
		
		/// <summary>
		/// Gets query results
		/// </summary>
		/// <returns>A singular <see cref="Artefacts.Artefact"/></returns>
		/// <param name="queryId">Query identifier</param>
		public Artefact QueryResult(object queryId)
		{
			Artefact artefact = _queryCache[queryId].Single();
			int id = artefact.Id.Value;
			if (!_artefactCache.ContainsKey(id))
				_artefactCache.Add(id, artefact);
			else
				_artefactCache[id] = artefact;
			return artefact;
		}
		
		/// <summary>
		/// Queries the results.
		/// </summary>
		/// <returns>The results</returns>
		/// <param name='queryId'>Query identifier</param>
		/// <param name='startIndex'>Start index</param>
		/// <param name='count'>Count</param>
		/// <remarks>
		/// With the <see cref="Queryable<>"/> being serializable so can be created on server and returned to client,
		/// it might be efficient and practical to retrieve just Artefact Id's in an array using a method like this, then in
		/// Queryable.this[index] use <see cref="ArtefactRepository.GetById"/> to retrieve each artefact as required??
		/// 	-	Think about this properly
		/// </remarks>
		public Artefact[] QueryResults(object queryId, int startIndex = 0, int count = -1)
		{
			Artefact[] results = (count == -1 ? // TODO: Is NhQueryable's caching sufficient here or should I use Queryable<>
				_queryCache[queryId].Skip(startIndex) : // with a new custom server-side query provider, and implement caching??
				_queryCache[queryId].Skip(startIndex).Take(count)).ToArray();
			foreach (Artefact artefact in results)
			{
				int id = artefact.Id.Value;
				if (_artefactCache.ContainsKey(id))
				{
					if (_artefactCache[id] != artefact)
						_artefactCache[id].CopyMembersFrom(artefact);
				}
				else
					_artefactCache[id] = artefact;
			}
			return results;
		}

		public object QueryExecute(byte[] binary)
		{
			try
			{
				BinaryFormatter bf = new BinaryFormatter();// { Binder = new QuerySerializationBinder() };
				ExpressionNode en = (ExpressionNode)bf.Deserialize(new System.IO.MemoryStream(binary));
				Expression expression = en.ToExpression();
				return Artefacts.Provider.Execute(expression);
			}
			catch (Exception ex)
			{
				throw Error(ex, binary);
			}
			
		}
		#endregion
		
		#region Collections/Enumerables/Queryables
		public IQueryable<Artefact> Artefacts { get; private set; }
		
		public IDictionary<Type, IQueryable> Queryables { get; private set; }
		#endregion
		
		#region Get/Set default paging options
		public PagingOptions GetDefaultPagingOptions()
		{
			throw new NotImplementedException();
		}

		public void SetDefaultPagingOptions(PagingOptions pagingOptions)
		{
			throw new NotImplementedException();
		}
		#endregion
		#endregion
		
		/// <summary>
		/// Construct a <see cref="FaultException"/> describing the server side exception <paramref name="ex"/> for
		/// communication to a client.
		/// </summary>
		/// <param name="ex">The exception that occurred on the server</param>
		/// <param name="details">Optional arbitrary data associated with the exception</param>
		/// <remarks>
		/// Have been playing around with this a little, to see what can be passed back to the clients. Unsuccessful
		/// in transmitting inner exceptions and data dictionary in any exceptions. Not sure why. Does transmit the top
		/// level exception (except for data dictionary) back to the client correctly, which will do for now.
		/// 	-	Will look into these issues some point. Without inner exceptions debugging server code can be difficult.
		///		(Server does, however, write exceptions, including inner exceptions, to console for now for debug purposes)
		/// </remarks>
		private FaultException Error(Exception ex, params object[] details)
		{
			StackFrame errorFrame = new StackFrame(1, true);
			MethodBase mb = errorFrame.GetMethod();
			ParameterInfo[] pi = errorFrame.GetMethod().GetParameters();
			StringBuilder action = new StringBuilder();
			action.AppendFormat("{0}:{1}.{2}(", mb.DeclaringType.Assembly.GetName().Name, mb.DeclaringType.FullName, mb.Name);
			for (int i = 0; i < pi.Length; i++)
				action.AppendFormat("\n\t{0}={1}{2}{3}", pi[i].Name, details[i].ToString().Contains('\n') ? "\n\t  " : "",
					details[i].ToString().Replace("\n", "\n\t  "), i == pi.Length - 1 ? "" : ",");
			action.AppendFormat("){0}[{1}:{2},{3}]:\n{4}: {5}\n{6}\n", pi.Length > 0 ? "\n" : " ",
				errorFrame.GetFileName(), errorFrame.GetFileLineNumber(), errorFrame.GetFileColumnNumber(),
				ex.GetType().FullName, ex.Message, ex.StackTrace);
			string actionStr = action.ToString();
			if (Configuration.OutputExceptions)
				Console.WriteLine("\n{0}\n", actionStr);
			FaultException<ExceptionDetail> retEx = new FaultException<ExceptionDetail>(
				new ExceptionDetail(ex), actionStr, // ex.GetType().FullName + " caught on ArtefactService",
				FaultCode.CreateSenderFaultCode(errorFrame.GetMethod().Name, mb.DeclaringType.FullName),
				OperationContext.Current.IncomingMessageHeaders.Action);
			foreach (object detail in details)
				retEx.Data.Add(retEx.Data.Count, detail);			
			return retEx;
		}
	}
}
