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
	[ServiceContract(Name = "IArtefactService", Namespace = "http://teknowledge.net.au/Artefacts/Service/",
		SessionMode = SessionMode.Allowed, ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
	[ServiceKnownType("GetArtefactTypes", typeof(Artefact))]
	public interface IArtefactService
	{
		/// <summary>
		/// Root artefact collection
		/// </summary>
		IQueryable<Artefact> Artefacts { get; }
		
		#region Add/Get/Exists/Update/Remove singular artefact operations
		/// <summary>
		/// Checks for the existence of an <see cref="Artefact"/> with the given URI
		/// </summary>
		/// <param name="uri"><see cref="Artefact"/> URI</param>
		/// <returns><c>true</c> if found, otherwise <c>false</c></returns>
		[OperationContract]
		bool Exists(Uri uri);

		/// <summary>
		/// Checks for the existence of an <see cref="Artefact"/> with the given identifier
		/// </summary>
		/// <param name="id"><see cref="Artefact"/> identifier</param>
		/// <returns><c>true</c> if found, otherwise <c>false</c></returns>
		[OperationContract]
		bool Exists(int id);

		/// <summary>
		/// Gets an <see cref="Artefact"/> with the given URI
		/// </summary>
		/// <param name="uri"><see cref="Artefact"/> URI</param>
		/// <returns>The retrieved <see cref="Artefact"/></returns>
		[OperationContract]
		Artefact Get(Uri uri);

		/// <summary>
		/// Gets an <see cref="Artefact"/> with the given identifier
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <returns>The retrieved <see cref="Artefact"/></returns>
		[OperationContract]
		Artefact Get(int id);
		
		/// <summary>
		/// Add the specified artefact.
		/// </summary>
		/// <param name="uri"><see cref="Artefact"/> URI</param>
		/// <returns>The new <see cref="Artefact"/></returns>
		[OperationContract]
		Artefact Add(Uri uri);

		/// <summary>
		/// Update the specified artefact.
		/// </summary>
		/// <param name="artefact">The <see cref="Artefact"/> to update</param>
		/// <remarks>
		/// In this experimental (iffy, dodgy) new approach, this would only need to update the aspects of the artefact if anything? (and the timestamps)
		/// </remarks>
		[OperationContract]
		void Update(Artefact artefact);

		/// <summary>
		/// Remove the specified artefact.
		/// </summary>
		/// <param name="artefact">The <see cref="Artefact"/> to remove</param>
		[OperationContract]
		void Remove(Artefact artefact);
		
		/// <summary>
		/// Remove the specified artefact.
		/// </summary>
		/// <param name="uri">The URI of the <see cref="Artefact"/> to remove</param>
		[OperationContract]
		void Remove(Uri uri);
		
		/// <summary>
		/// Remove the specified artefact.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="Artefact"/> to remove</param>
		[OperationContract]
		void Remove(int id);
		#endregion
		
		#region Expression / Query Methods
		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="expression">Expression.</param>
		[OperationContract]
		object QueryPreload(byte[] expression);

		/// <summary>
		/// Queries the results.
		/// </summary>
		/// <returns>The results.</returns>
		/// <param name="queryId">Query identifier.</param>
		/// <param name="startIndex">Start index.</param>
		/// <param name="count">Count.</param>
		[OperationContract]
		int[] QueryResults(object queryId, int startIndex = 0, int count = -1);

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

