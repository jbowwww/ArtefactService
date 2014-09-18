using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Reflection;

using NHibernate.Criterion;

using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

namespace Artefacts.Service
{
	/// <summary>
	/// Repository interface
	/// </summary>
	/// <remarks>
	/// 
	/// Various removed experimental methods:
	/// //		[OperationContract]
	///		int QueryCount(object queryId);
	///		
	///		[OperationContract]
	///		TArtefact QueryResult(object queryId);
	///				
	/// [OperationContract]
	///	object QueryMethodCall(object queryId, string methodName);// MethodInfo method);
	///
	///	[OperationContract]
	///	object QueryExecute(byte[] binary);
	///
	/// </remarks>
	[ServiceContract(
		Name = "IRepository",
		Namespace = "http://teknowledge.net.au/Artefacts/Service/",
		SessionMode = SessionMode.Allowed,
		ProtectionLevel = System.Net.Security.ProtectionLevel.None
//		CallbackContract = typeof()
	)]
	[ServiceKnownType("GetArtefactTypes", typeof(Artefact))]
	public interface IRepository
	{
		#region Basic Service Operations
		/// <summary>
		/// Connect the specified host.
		/// </summary>
		/// <param name="host">Host.</param>
		[OperationContract(IsInitiating=true)]
		int Connect(Host host);
		
		/// <summary>
		/// Disconnect the specified clientId.
		/// </summary>
		/// <param name="clientId">Client identifier.</param>
		[OperationContract(IsTerminating=true)]
		void Disconnect(Host host);
		#endregion
		
		#region Collections/Enumerables/Queryables
		/// <summary>
		/// Root artefact collection
		/// </summary>
		IQueryable<Artefact> Artefacts { get; }

		/// <summary>
		/// IQueryable root for each Type
		/// </summary>
		IDictionary<Type, IQueryable> Queryables { get; }
		#endregion
		
		#region Add/Get/Exists/Update/Remove singular artefact operations
		/// <summary>
		/// Add the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		[OperationContract]
		int Add(Artefact artefact);

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="artefact">Artefact.</param>
		[OperationContract]
		int GetId(Artefact artefact);

		/// <summary>
		/// Gets the by identifier.
		/// </summary>
		/// <returns>The by identifier.</returns>
		/// <param name="id">Identifier.</param>
		[OperationContract]
		Artefact GetById(int id, Type T = null);

		/// <summary>
		/// Update the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		[OperationContract]
		void Update(Artefact artefact);

		/// <summary>
		/// Remove the specified artefact.
		/// </summary>
		/// <param name="artefact">Artefact.</param>
		[OperationContract]
		void Remove(Artefact artefact);
		#endregion
		
		#region Query Methods
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <returns>The query async result</returns>
		/// <param name="expression">Expression.</param>
		[OperationContract(IsOneWay=true, ReplyAction="http://teknowledge.net.au/Artefacts/Service/ResponseQueryPreload/")]
		void QueryPreload(byte[] expression);

		/// <summary>
		/// Queries the results.
		/// </summary>
		/// <returns>The results.</returns>
		/// <param name="queryId">Query identifier.</param>
		/// <param name="startIndex">Start index.</param>
		/// <param name="count">Count.</param>
		[OperationContract]
		QueryResult QueryResults(object queryId, int startIndex = 0, int count = -1);

		/// <summary>
		/// Queries the execute.
		/// </summary>
		/// <returns>The execute.</returns>
		/// <param name="expression">Expression.</param>
		[OperationContract]
		object QueryExecute(byte[] expression);
		#endregion
	}
}

