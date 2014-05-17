using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
//using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
//using System.ServiceModel.Description;
//using System.ServiceModel.Dispatcher;
using System.Text;

using NHibernate;
using NHibernate.Linq;
using Serialize.Linq;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using Serialize.Linq.Serializers;

namespace Artefacts.Service
{
	/// <summary>
	/// Artefact repository
	/// </summary>
	/// <remarks>
	///	-	Removed parameters of <see cref="ServiceBehaviourAttribute"/>:
	///			MaxItemsInObjectGraph=100,
	///			ReleaseServiceInstanceOnTransactionComplete=false)]
	/// </remarks>
	[ServiceBehavior(
				IncludeExceptionDetailInFaults=true,
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
		private Dictionary<object, int> _countCache;
//		private Dictionary<object, QueryResult<Artefact>> _queryResultCache;
		private ConcurrentQueue<object> _queryExecuteQueue;
		private INhQueryProvider _nhQueryProvider;
		private BinaryFormatter _binaryFormatter;
		#endregion

		public ArtefactServiceConfiguration Configuration { get; private set; }
		
		public ExpressionVisitor QueryVisitor { get; protected set; }
		
		public ArtefactRepository()
		{
			Configuration = new ArtefactServiceConfiguration();

			_artefactCache = new Dictionary<int, Artefact>();
			QueryCache = new Dictionary<object, IQueryable>();
			_countCache = new Dictionary<object, int>();
//			_queryResultCache = new Dictionary<object, QueryResult<Artefact>>();
			_queryExecuteQueue = new ConcurrentQueue<object>();

			_nhQueryProvider = new DefaultQueryProvider(Session.GetSessionImplementation());
			_binaryFormatter = new BinaryFormatter();
			QueryVisitor = new ServerQueryVisitor(this);
			
			Expression expression = 
				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(Artefact) },
				Expression.Call(typeof(ArtefactRepository).GetProperty("Session", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
			Artefacts = new NhQueryable<Artefact>(_nhQueryProvider, expression);
//				_nhQueryProvider.CreateQuery<Artefact>(expression);
//			Session.Query<Artefact>();
//				new Queryable<Artefact>(this, _nhQueryProvider, expression);
//				new QueryableNhProxy<Artefact>(new NhQueryable<Artefact>(_nhQueryProvider, Expression.Empty));			//				(NhQueryable<Artefact>)_nhQueryProvider.CreateQuery<Artefact>(Expression.Empty));
			Queryables = new ConcurrentDictionary<Type, IQueryable>();
			Queryables.Add(typeof(Artefact), Artefacts);
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
//		public object CreateQuery(byte[] binary)
//		{
//			try
//			{
//				Expression expression = ((ExpressionNode)_binaryFormatter.Deserialize(new System.IO.MemoryStream(binary))).ToExpression();
//				if (expression.Type.GetInterface("System.Collections.IEnumerable") == null)
//					throw new ArgumentOutOfRangeException("expression", expression, "Not IEnumerable");
//				object queryId = expression.Id();				//.ToString();
//				if (!QueryCache.ContainsKey(queryId))
//				{
//					IQueryable<Artefact> q =
////						new NhQueryable<Artefact>(Artefacts.Provider, expression);
////						Artefacts.Provider.Execute<IQueryable<Artefact>>(expression);
//					Artefacts.Provider.CreateQuery<Artefact>(expression);
//					QueryCache.Add(queryId, q);					//(Queryable<Artefact>)				// Session.Query<Artefact>().Provider.CreateQuery<Artefact>(en.ToExpression());
//				}
//				return queryId;
//			}
//			catch (Exception ex)
//			{
//				throw Error(ex, binary);
//			}
//		}
		
//		public int QueryCount(object queryId)
//		{
//			try
//			{
//				IQueryable<Artefact> query = QueryCache[queryId];
//				int count = _countCache.ContainsKey(queryId) ?
//					_countCache[queryId] :
//					_countCache[queryId] = query.Count();
//	//					_nhQueryProvider.Execute<int>(query.Expression);
//				return count;
//			}
//			catch (Exception ex)
//			{
//				throw Error(ex, queryId);
//			}
//		}
//		
//		public Artefact QueryResult(object queryId)
//		{
//			try
//			{
//				IQueryable<Artefact> query = QueryCache[queryId];
//				Artefact artefact = _nhQueryProvider.Execute<Artefact>(query.Expression);
//				return artefact;
//			}
//			catch (Exception ex)
//			{
//				throw Error(ex, queryId);
//			}
//		}
//		
		public Artefact[] QueryResults(object queryId, int startIndex = 0, int count = -1)
		{
			try
			{	
				IQueryable<Artefact> query = (IQueryable<Artefact>)QueryCache[queryId];
				Artefact[] results = (count == -1 ? // TODO: Is NhQueryable's caching sufficient here or should I use Queryable<>
					query.Skip(startIndex) : // with a new custom server-side query provider, and implement caching??
					query.Skip(startIndex).Take(count)).ToArray();
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
			catch (Exception ex)
			{
				throw Error(ex, queryId, startIndex, count);
			}
		}
//		
//		public object QueryMethodCall(object queryId, string methodName)//  MethodInfo method)
//		{
//			try
//			{
//				IQueryable<Artefact> query = QueryCache[queryId];
//				string[] methodFullName = methodName.Split(':');
//				MethodInfo method = typeof(System.Linq.Enumerable).GetMethod(methodFullName[1], new Type[] { typeof(IEnumerable<Artefact>) });
//					//Type.GetType(methodFullName[0]).GetMethod(methodFullName[1]);
//				object result = method.Invoke(query, new object[] { });		// currently can't use methods with parameters - need to serialize them
//				return result;
//			}
//			catch (Exception ex)
//			{
//				throw Error(ex, queryId);
//			}
//		}
		
		public object QueryExecute(object query)
		{
			try
			{
				if (!(query is ExpressionNode))		// string))
					throw new ArgumentOutOfRangeException("query", query, "Should be an ExpressionNode");	// JSON string");
//				ExpressionNode en = new JsonSerializer().Deserialize<ExpressionNode>((string)query);
//				new Serialize.Linq.Serializers.ExpressionSerializer(new JsonSerializer()).DeserializeText((string)query);
				Expression expression = ((ExpressionNode)query).ToExpression();
					// new JsonSerializer().Deserialize<Expression>((string)query);
//				Expression expression = ((ExpressionNode)_binaryFormatter.Deserialize(new System.IO.MemoryStream(binary))).ToExpression();

				object result = Artefacts.Provider.Execute(expression);
				return result;
			}
			catch (Exception ex)
			{
				throw Error(ex, query);	// binary);
			}			
		}
		#endregion
		
		#region Collections/Enumerables/Queryables
		public IQueryable<Artefact> Artefacts { get; private set; }
		
		public IDictionary<Type, IQueryable> Queryables { get; private set; }

		public Dictionary<object, IQueryable> QueryCache { get; private set; }
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
