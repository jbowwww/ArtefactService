using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using Serialize.Linq;
using Serialize.Linq.Nodes;
using NHibernate;
using NHibernate.Linq;

namespace Artefacts.Service
{
	/// <summary>
	/// Artefact repository
	/// </summary>
	/// <remarks>
	/// 	- Should the root IQueryable ("Artefacts") (or any other near-root queryables) on the client side
	/// 		be constructed using MemberExpressions referring to <see cref="IRepository"/>  ??
	/// 
	///	-	Removed parameters of <see cref="ServiceBehaviourAttribute"/>:
	///			//			AddressFilterMode = add,
	///			ConfigurationName = "",
	///			IgnoreExtensionDataObject = true,
	///			MaxItemsInObjectGraph = 0, 
	///			Namespace = "", Name = "",
	///			ReleaseServiceInstanceOnTransactionComplete = false,
	///			TransactionAutoCompleteOnSessionClose = false,
	///			TransactionTimeout = "",
	///			UseSynchronizationContext = true,
	///			ValidateMustUnderstand = false,
	/// 
	/// 	- Removed experimental member method operation definitions:
	/// 
	///		public object CreateQuery(byte[] binary)
	///		{
	///			try
	///			{
	///				Expression expression = ((ExpressionNode)_binaryFormatter.Deserialize(new System.IO.MemoryStream(binary))).ToExpression();
	///				if (expression.Type.GetInterface("System.Collections.IEnumerable") == null)
	///					throw new ArgumentOutOfRangeException("expression", expression, "Not IEnumerable");
	///				object queryId = expression.Id();				//.ToString();
	///				if (!QueryCache.ContainsKey(queryId))
	///				{
	///					IQueryable<Artefact> q =
	///						new NhQueryable<Artefact>(Artefacts.Provider, expression);
	///					Artefacts.Provider.Execute<IQueryable<Artefact>>(expression);
	///					Artefacts.Provider.CreateQuery<Artefact>(expression);/
	///					QueryCache.Add(queryId, q);					//(Querya/ble<Artefact>)				// Session.Query<Artefact>().Provider.CreateQuery<Artefact>(en.ToExpression());
	///				/}
	///				return queryId;
	///			}
	///			catch (Exception ex)
	///			{
	///				throw Error(ex, binary);
	///			}
	///		}
	/// 
	/// //			Expression expression = 
	///				Expression.Call(typeof(LinqExtensionMethods), "Query", new Type[] { typeof(Artefact) },
	///				Expression.Call(typeof(ArtefactRepository).GetProperty("Session", BindingFlags.Public | BindingFlags.Static).GetGetMethod()));
	///			Artefacts = new NhQueryable<Artefact>(_nhQueryProvider, expression);
	///			new NhQueryable<Artefact>(_nhQueryProvider, Expression.Variable(typeof(IQueryable<Artefact>), "Artefacts"));
	///				_nhQueryProvider.CreateQuery<Artefact>(expression);
	///			Session.Query<Artefact>();
	///				new Queryable<Artefact>(this, _nhQueryProvider, expression);
	///				new QueryableNhProxy<Artefact>(new NhQueryable<Artefact>(_nhQueryProvider, Expression.Empty));			//				(NhQueryable<Artefact>)_nhQueryProvider.CreateQuery<Artefact>(Expression.Empty));
	/// </remarks>
	[ServiceBehavior(IncludeExceptionDetailInFaults=true,
		InstanceContextMode=InstanceContextMode.Single,
		ConcurrencyMode=ConcurrencyMode.Multiple)]
	[ServiceKnownType("GetArtefactTypes", typeof(Artefact))]
	public class Repository : IRepository, IDisposable
	{		
		#region Static members
		/// <summary>
		/// Constant artefact update age limit.
		/// </summary>
		/// <remarks>May not remain constant - make configurable by clients</remarks>
		public static TimeSpan ArtefactUpdateAgeLimit { get { return Artefact.UpdateAgeLimit; } }

		/// <summary>
		/// Gets the transaction.
		/// </summary>
		/// <value>The transaction.</value>
		public static ITransaction Transaction { get { return Session.Transaction; } }

		/// <summary>
		/// Gets the session.
		/// </summary>
		/// <value>The session.</value>
		public static ISession Session { get { return NhBootStrap.Session; } }

		/// <summary>
		/// Gets or sets the context.
		/// </summary>
		/// <value>The context.</value>
		[ThreadStatic]
		private static IRepository _context = null;
		public static IRepository Context {
			get { return _context; }
			set { _context = value; }
		}
		#endregion

		#region Private fields
		private Random _randomGenerator;
		private Dictionary<int, Artefact> _artefactCache;
		private Dictionary<object, int> _countCache;
//		private Dictionary<object, QueryResult<Artefact>> _queryResultCache;
		private ConcurrentQueue<object> _queryExecuteQueue;
		private INhQueryProvider _nhQueryProvider;
		private BinaryFormatter _binaryFormatter;
		#endregion

		#region Protected fields
		/// <summary>
		/// Gets the configuration.
		/// </summary>
		protected readonly ArtefactServiceConfiguration Configuration;

		/// <summary>
		/// Gets or sets the query visitor.
		/// </summary>
		protected ExpressionVisitor QueryVisitor;
		#endregion

		#region Public properties
		/// <summary>
		/// Gets the query cache.
		/// </summary>
		public Dictionary<object, IQueryable> QueryCache { get; private set; }

		#region Collections/Enumerables/Queryables
		/// <summary>
		/// Root artefact collection
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IQueryable<Artefact> Artefacts { get; private set; }

		/// <summary>
		/// IQueryable root for each Type
		/// </summary>
		/// <remarks>IRepository implementation</remarks>
		public IDictionary<Type, IQueryable> Queryables { get; private set; }
		#endregion
		#endregion

		#region Construction & disposal
		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.ArtefactRepository"/> class.
		/// </summary>
		public Repository()
		{
			Context = this;
			_randomGenerator = new Random(DateTime.Now.Second);
			_artefactCache = new Dictionary<int, Artefact>();
			_countCache = new Dictionary<object, int>();
			//_queryResultCache = new Dictionary<object, QueryResult<Artefact>>();
			_queryExecuteQueue = new ConcurrentQueue<object>();
			_nhQueryProvider = new DefaultQueryProvider(Session.GetSessionImplementation());
			_binaryFormatter = new BinaryFormatter();

			Configuration = new ArtefactServiceConfiguration();
			QueryVisitor = new ServerQueryVisitor(this);
			QueryCache = new Dictionary<object, IQueryable>();
			Artefacts = Session.Query<Artefact>();
			Queryables = new ConcurrentDictionary<Type, IQueryable>();
			Queryables.Add(typeof(Artefact), Artefacts);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Artefacts.Service.Repository"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Artefacts.Service.Repository"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Artefacts.Service.Repository"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="Artefacts.Service.Repository"/> so
		/// the garbage collector can reclaim the memory that the <see cref="Artefacts.Service.Repository"/> was occupying.</remarks>
		public void Dispose()
		{
			
		}
		#endregion
		
		#region Basic service operations
		public int Connect(Host host)
		{
			if (host.Connected)
				throw Error(new ApplicationException("Already connected"), host.ConnectionId);
			host.ConnectionId = _randomGenerator.Next(1, int.MaxValue);
			if (host.IsTransient)
				Add(host);
			else
				Update(host);
			Console.WriteLine("\nConnect({0} #{1}) = {2}", typeof(Host).FullName, host.Id, host.ConnectionId);
			return host.ConnectionId;
		}
		
		public void Disconnect(Host host)
		{
			if (host.IsTransient)
				throw Error(new ApplicationException("Cannot disconnect transient host"));
			if (!host.Connected)
				throw Error(new ApplicationException("Not connected"), host.ConnectionId);
			Console.WriteLine("Disconnect({0} #{1} [connectionId={2}])\n", typeof(Host).FullName, host.Id, host.ConnectionId);
			host.ConnectionId = -1;
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
			Console.WriteLine("Add({0} artefact #{1})", artefact.GetType().FullName, artefact.Id);
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
				Console.WriteLine(ex.ToString());
				throw Error(ex, artefact);
			}
			finally
			{
				if (transaction != null)
					transaction.Dispose();
			}
		}

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public int GetId(Artefact artefact)
		{
			Console.WriteLine("GetId({0} artefact #{1})", artefact.GetType().FullName, artefact.Id);
			try
			{
				return artefact.IsTransient ? (artefact.Id = (int)Session.GetIdentifier(artefact)).Value : artefact.Id.Value;
			}
			catch (Exception ex)
			{
				throw Error(ex, artefact);
			}
		}

		/// <summary>
		/// Gets the by identifier.
		/// </summary>
		/// <returns>The by identifier.</returns>
		/// <param name="id">Identifier.</param>
		/// <remarks>IRepository implementation</remarks>
		public Artefact GetById(int id)
		{
			Console.WriteLine("GetById(#{0})", id);
			try
			{
//				return _artefactCache.ContainsKey(id) && (_artefactCache[id].UpdateAge > ArtefactUpdateAgeLimit)
//					? _artefactCache[id] : _artefactCache[id] = Session.Get<Artefact>(id);
				Artefact artefact;
				if (_artefactCache.ContainsKey(id) && _artefactCache[id].UpdateAge < ArtefactUpdateAgeLimit)
					artefact = _artefactCache[id];
				else
					artefact = _artefactCache[id] = Session.Get<Artefact>(id);
					
				if (artefact.Id != id)
					throw new ApplicationException(string.Format("GetById(id={0}).Id != {0}", id));
//				Type T = artefact.GetType();
				return artefact;
			}
			catch (Exception ex)
			{
				throw Error(ex, id);
			}
		}

		/// <summary>
		/// Update the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public void Update(Artefact artefact)
		{
			Console.WriteLine("Update({0} artefact #{1})", artefact.GetType().FullName, artefact.Id);
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

		/// <summary>
		/// Remove the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		/// <remarks>IRepository implementation</remarks>
		public void Remove(Artefact artefact)
		{
			Console.WriteLine("Remove({0} artefact #{1})", artefact.GetType().FullName, artefact.Id);
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
		/// Creates the query.
		/// </summary>
		/// <returns>The query identifier</returns>
		/// <param name="expression">Expression.</param>
		public object QueryPreload(byte[] expression)	//ExpressionNode expression)
		{
			Expression serverSideExpression = null;
			object serverId = null;
			IQueryable serverSideQuery = null;
			try
			{
				serverSideExpression = QueryVisitor.Visit(expression.FromBinary());
				serverId = serverSideExpression.Id();
				Console.WriteLine("QueryPreload([{0}] \"{1}\")", serverId, serverSideExpression);
				if (!QueryCache.ContainsKey(serverId))
				{
					serverSideQuery = _nhQueryProvider.CreateQuery(serverSideExpression);				
					QueryCache[serverId] = serverSideQuery;
				}
			}
			catch (Exception ex)
			{
				Console.Write("\n--- Repository Exception ---\n{0}: {1}\nStackTrace:\n  {2}\n\n",
					ex.GetType().FullName, ex.Message, ex.StackTrace.Trim('\n').Insert(0, "\n").Replace("\n", "\n  "));
				FaultException fEx = Error(ex);
				fEx.Data.Add("serverSideExpression", expression.NodeFromBinary().ToString());// serverSideExpression.ToString());
				fEx.Data.Add("serverId", serverId.ToString());
				fEx.Data.Add("serverSideQuery", serverSideQuery.ToString());
				throw fEx;
			}
			return serverId;
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
			IQueryable<Artefact> serverSideQuery = null;
			int[] results = null;
			try
			{
				serverSideQuery = (IQueryable<Artefact>)QueryCache[queryId];
				Console.WriteLine("QueryResults({0})\n  expression = \"{1}\"", queryId, serverSideQuery.Expression);
				
				results = (count == -1 ? // TODO: Is NhQueryable's caching sufficient here or should I use Queryable<>
					serverSideQuery.Skip(startIndex) : // with a new custom server-side query provider, and implement caching??
					serverSideQuery.Skip(startIndex).Take(count)).Select((a) => a.Id.Value).ToArray();
//				foreach (int artefactId in results)
//				{
//					Artefact artefact = GetById(artefactId);
//					if (_artefactCache.ContainsKey(artefactId))
//					{
//						if (_artefactCache[artefactId] != artefact)
//							_artefactCache[artefactId].CopyMembersFrom(artefact);
//					}
//					else
//						_artefactCache[artefactId] = artefact;
//				}
				return results;
			}
			catch (Exception ex)
			{
				Console.Write("\n--- Repository Exception ---\n{0}: {1}\nStackTrace:\n  {2}\n\n",
					ex.GetType().FullName, ex.Message, ex.StackTrace.Trim('\n').Insert(0, "\n").Replace("\n", "\n  "));
				FaultException fEx = Error(ex);
				fEx.Data.Add("queryId", queryId.ToString());
				fEx.Data.Add("startIndex", startIndex.ToString());
				fEx.Data.Add("count", count.ToString());
				fEx.Data.Add("serverSideQuery", serverSideQuery.ToString());
				fEx.Data.Add("results", results.ToString());
				throw fEx;
			}
		}

		/// <summary>
		/// Queries the execute.
		/// </summary>
		/// <returns>The execute.</returns>
		/// <param name="binary">Binary.</param>
		/// <param name="expression">Expression</param>
		public object QueryExecute(byte[] expression)
		{
			Expression serverSideExpression = null;
			object result = null;
			try
			{
				serverSideExpression = QueryVisitor.Visit(expression.FromBinary());
				Console.WriteLine("QueryExecute([{0}] \"{1}\") = ", serverSideExpression.Id(), serverSideExpression);
				result = _nhQueryProvider.Execute(serverSideExpression);
				return result;
			}
			catch (Exception ex)
			{
				Console.Write("\n--- Repository Exception ---\n{0}: {1}\nStackTrace:\n  {2}\n\n",
					ex.GetType().FullName, ex.Message, ex.StackTrace.Trim('\n').Insert(0, "\n").Replace("\n", "\n  "));
				FaultException fEx = Error(ex);
				fEx.Data.Add("serverSideExpression", expression.NodeFromBinary().ToString());// serverSideExpression.ToString());
				fEx.Data.Add("result", result.ToString());
				throw fEx;
			}			
		}
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
			MethodBody methodBody = mb.GetMethodBody();
//			ParameterInfo[] pi = mb.GetParameters();
			StringBuilder action = new StringBuilder();
			action.AppendFormat("{0}:{1}.{2}(", mb.DeclaringType.Assembly.GetName().Name, mb.DeclaringType.FullName, mb.Name);
//			for (int i = 0; i < pi.Length; i++)
//				action.AppendFormat("\n\t{0}={1}{2}{3}", pi[i].Name, details[i].ToString().Contains('\n') ? "\n\t  " : "",
//					details[i].ToString().Replace("\n", "\n\t  "), i == pi.Length - 1 && methodBody.LocalVariables.Count == 0 ? "" : ",");
			foreach (LocalVariableInfo lvInfo in methodBody.LocalVariables)
				action.AppendFormat("\n\t{0}", lvInfo.ToString());
			action.AppendFormat("){0}[{1}:{2},{3}]:\n{4}: {5}\n{6}\n", /*pi.Length > 0 ?*/ "\n", // : " ",
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
