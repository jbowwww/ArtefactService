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
using WCFChannel = System.ServiceModel.Channels;
using System.Reflection;
using System.Diagnostics;

using Serialize.Linq;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

using NHibernate;
//using NHibernate.Cfg;
using NHibernate.Linq;
//using NHibernate.Criterion;

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
	[ServiceBehavior(
		IncludeExceptionDetailInFaults=true,
		InstanceContextMode=InstanceContextMode.Single,
		ConcurrencyMode=ConcurrencyMode.Single)]
	public class ArtefactRepository :
		IRepository<Artefact>
	{
//		#region Static members (store and return Type arrays for WCF service known types)
//		private static List<Type> _artefactTypes = null;
//		public static List<Type> ArtefactTypes {
//			get
//			{
//				return _artefactTypes != null ? _artefactTypes : _artefactTypes = new List<Type>();
//			}
//		}
//		public static Type[] GetArtefactTypes(ICustomAttributeProvider provider)
//		{
//			ServiceKnownTypeAttribute[] staticKnownTypes = provider == null ?
//				new ServiceKnownTypeAttribute[] {} :
//				(ServiceKnownTypeAttribute[])provider.GetCustomAttributes(typeof(ServiceKnownTypeAttribute), true);
//			ServiceKnownTypeAttribute[] hardcodeKnownTypes = new ServiceKnownTypeAttribute[]
//			{
//				new ServiceKnownTypeAttribute(typeof(NhQueryable<Artefact>))
//			};
//			Type[] knownTypes = new Type[ArtefactTypes.Count + staticKnownTypes.Length + hardcodeKnownTypes.Length];
//			Array.ConvertAll<ServiceKnownTypeAttribute, Type>(staticKnownTypes,
//				(input) => input.Type).CopyTo(knownTypes, 0);
//			Array.ConvertAll<ServiceKnownTypeAttribute, Type>(hardcodeKnownTypes,
//				(input) => input.Type).CopyTo(knownTypes, staticKnownTypes.Length);
//			ArtefactTypes.CopyTo(knownTypes, hardcodeKnownTypes.Length);
//			return knownTypes;
//		}
//		#endregion
		
		#region Static members
//		private static ISession _session;
		
		public static ITransaction Transaction {
			get { return Session.Transaction; }
		}
		
		public static ISession Session {
			get { return NhBootStrap.Session; }
//			{
//				ISession ret = _session != null && _session.IsOpen ? _session : _session = NhBootStrap.Session;
//				if (ret == null)
//					throw new NullReferenceException("ArtefactService.Session == null");
//				return ret;
//			}
		}
		#endregion
		
		#region Private fields
		private Dictionary<object, IQueryable<Artefact>> _queryCache;
		private Dictionary<object, ICriteria> _queryCritCache;
		private Dictionary<object, IQuery> _queryHqlCache;
		private Dictionary<object, QueryResult<Artefact>> _queryResultCache;
		private ConcurrentQueue<object> _queryExecuteQueue;
		#endregion
		
		public ArtefactRepository()
		{
//			_session = null;
			_queryCache = new Dictionary<object, IQueryable<Artefact>>();
			_queryCritCache = new Dictionary<object, ICriteria>();
			_queryHqlCache = new Dictionary<object, IQuery>();
			_queryResultCache = new Dictionary<object, QueryResult<Artefact>>();
//			Transaction = null;
		}

		#region IRepository[Artefact] implementation
		
		#region Add/Get/Update/Remove singular artefact operations
		public int Add(Artefact artefact)
		{
			ITransaction transaction = null;
			try
			{
				if (Session.Transaction == null || !Session.Transaction.IsActive)
					transaction = Session.BeginTransaction();
				int id = (int)Session.Save(artefact);
				if (transaction != null)
					transaction.Commit();
				return id;
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				throw Error(ex, artefact); 		//new KeyValuePair<string, object>("artefact", artefact));
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
				return (int)Session.GetIdentifier(artefact);
			}
			catch (Exception ex)
			{
				throw Error(ex, artefact); 		// new KeyValuePair<string, object>("artefact", artefact));
			}
		}

		public Artefact GetById(int id)
		{
			try
			{
				return Session.Get<Artefact>(id);
			}
			catch (Exception ex)
			{
				throw Error(ex, id); 		// new KeyValuePair<string, object>("id", id));
			}
		}

		public void Update(Artefact artefact)
		{
			ITransaction transaction = null;
			try
			{
				if (Session.Transaction == null || !Session.Transaction.IsActive)
					transaction = Session.BeginTransaction();
				Update(artefact);
				if (transaction != null)
					transaction.Commit();
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				throw Error(ex, artefact); 		// new KeyValuePair<string, object>("artefact", artefact));
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
			try
			{
				if (Session.Transaction == null || !Session.Transaction.IsActive)
					transaction = Session.BeginTransaction();
				Session.Delete(artefact);
				if (transaction != null)
					transaction.Commit();
			}
			catch (Exception ex)
			{
				if (transaction != null)
					transaction.Rollback();
				throw Error(ex, artefact); 		// new KeyValuePair<string, object>("artefact", artefact));
			}
			finally
			{
				if (transaction != null)
					transaction.Dispose();
			}
		}
		#endregion
		
		#region Collections/Enumerables/Queryables
		public object CreateQuery(ExpressionNode expressionNode)
		{
			try
			{
//				System.Linq.Expressions.Expression expression = new Serialize.Linq.Serializers.JsonSerializer().Deserialize<System.Linq.Expressions.Expression>(expressionNode);
//				ExpressionNode expression = new Serialize.Linq.Serializers.JsonSerializer().Deserialize<ExpressionNode>(expressionNode);
				object queryId = expressionNode.GetHashCode();
//				System.Linq.Expressions.Expression expression = expressionNode.ToExpression();
				
				if (!_queryCache.ContainsKey(queryId))
				{
					IQueryable<Artefact> queryable = Session.Query<Artefact>().Provider.CreateQuery<Artefact>(expressionNode.ToExpression());
					_queryCache.Add(queryId, queryable);
				}
//				new System.Threading.Thread(
//					() =>
//				{
//					
//				});
				return queryId;
			}
			catch (Exception ex)
			{
				throw Error(ex, expressionNode); 		// new KeyValuePair<string, object>("expressionNode", expressionNode));
			}
		}
		
		public object CreateQuery_EN_Binary(byte[] binary)
		{
			try
			{
				BinaryFormatter bf = new BinaryFormatter() { Binder = new QuerySerializationBinder() };
				ExpressionNode en = (ExpressionNode)bf.Deserialize(new System.IO.MemoryStream(binary));		//, delegate(System.Runtime.Remoting.Messaging.Header[] headers) {	return null; });
				Expression expression = en.ToExpression();
				object queryId = en.ToString();
				if (!_queryCache.ContainsKey(queryId))
				{
					// TODO: I think I just need to traverse the expression tree from en.ToExpression(),
					// find the expression node representing the root query from the client side (RepositoryClientProxy.Artefacts)
					// The expression corresponding to that node is (currently, see source in mentioned class)  method call expression
					// to ISession.Query<>(). As a 1st parameter (maybe.. I think..?) this will take the ISession paramter. From client side
					// this is probably null, initialise it to this.Session before creating the query
					// Do this and this shit might even work?!
					// 16/1/14: ^^ It appears that this may well be false - may be almost working
					
//					IQueryable<Artefact> q = Session.Query<Artefact>().Provider.CreateQuery<Artefact>(en.ToExpression());
//					_queryCache.Add(queryId, q);
					
					IQueryable<Artefact> qBase;
					QueryResult<Artefact> qrBase;
					if (_queryCache.ContainsKey(string.Empty))
						qBase = _queryCache[string.Empty];
					else
					{
						qBase = new NhQueryable<Artefact>(Session.GetSessionImplementation());
						_queryCache.Add(string.Empty, qBase);
						qrBase = new QueryResult<Artefact>(qBase);
						_queryResultCache.Add(string.Empty, qrBase);
					}					
					
					IQueryable<Artefact> q = qBase.Provider.CreateQuery<Artefact>(expression);
					_queryCache.Add(queryId, q);
//					q.ToList().Count;
					
					QueryResult<Artefact> qr = new QueryResult<Artefact>(q);
					_queryResultCache.Add(queryId, qr);
					
				}
				return queryId;
			}
			catch (Exception ex)
			{
				throw Error(ex, binary);				//, new KeyValuePair<string, object>("binary", binary));
			}
		}
		
		public int QueryCount(object queryId)
		{
//			return _queryHqlCache[queryId].List<Artefact>().Count;
//			return _queryCritCache[queryId].List().Count;
//			int c = _queryCache[queryId].Count();		// COunt() extension method does not seem to be supported
//			return c;
//			(_queryCache[queryId] as NhQueryable<Artefact>).
			int c = _queryResultCache[queryId].Count;
			return c;
		}
		
		public Artefact[] QueryResults(object queryId, int startIndex = 0, int count = -1)
		{
			return (count == -1 ?
				_queryResultCache[queryId].Results.Skip(startIndex) :
				_queryResultCache[queryId].Results.Skip(startIndex).Take(count)).ToArray();
//			return (count == -1 ?
//				_queryHqlCache[queryId].List<Artefact>().Skip(startIndex) :
//				_queryHqlCache[queryId].List<Artefact>().Skip(startIndex).Take(count)).ToArray();
//			return (count == -1 ?
//				_queryCritCache[queryId].List<Artefact>().Skip(startIndex) :
//				_queryCritCache[queryId].List<Artefact>().Skip(startIndex).Take(count)).ToArray();
//			return (count == -1 ?
//				_queryCache[queryId].Skip(startIndex) :
//				_queryCache[queryId].Skip(startIndex).Take(count)).ToArray();
		}
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
			Console.WriteLine("{0}.{1} [{2}:{3},{4}]: Exception: {5}\n{6}\n",
				errorFrame.GetMethod().DeclaringType, errorFrame.GetMethod().Name, errorFrame.GetFileName(),
				errorFrame.GetFileLineNumber(), errorFrame.GetFileColumnNumber(), ex.GetType().FullName, ex.ToString());
			
			MethodBase mb = errorFrame.GetMethod();
			ParameterInfo[] pi = errorFrame.GetMethod().GetParameters();
			StringBuilder action = new StringBuilder(mb.DeclaringType.FullName);
			action.AppendFormat(".{0}(", mb.Name);
			for (int i = 0; i < pi.Length; i++)
				action.AppendFormat("{0}={1}{2}", pi[i].Name, details[i], i == pi.Length - 1 ? string.Empty : ", ");
			action.Append(")");
			
			FaultException<ExceptionDetail> retEx = new FaultException<ExceptionDetail>(
				new ExceptionDetail(ex),
				"Exception caught on ArtefactService",
				FaultCode.CreateSenderFaultCode(errorFrame.GetMethod().Name, mb.DeclaringType.FullName),
				action.ToString());					//				OperationContext.Current.IncomingMessageHeaders.Action);
		
			return retEx;							
		}
	}
}

//new FaultReason(string.Format("Fault exception: {0}: {1}", ex.GetType().Name, ex.Message)),
//				new FaultCode(ex.GetType().Name, ex.GetType().Namespace),
//				