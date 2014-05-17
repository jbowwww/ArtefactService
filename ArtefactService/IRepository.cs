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
	[ServiceContract]
	[ServiceKnownType(typeof(Queryable<Artefact>))]
	public interface IRepository<TArtefact> where TArtefact : Artefact
	{
		#region Collections/Enumerables/Queryables
		IQueryable<Artefact> Artefacts { get; }
		
		IDictionary<Type, IQueryable> Queryables { get; }
		#endregion
		
		#region Add/Get/Update/Remove singular artefact operations
		[OperationContract]
		int Add(TArtefact artefact);
		
		[OperationContract]
		int GetId(TArtefact artefact);
		
		[OperationContract]
		TArtefact GetById(int id);
		
		[OperationContract]
		void Update(TArtefact artefact);

		[OperationContract]
		void Remove(TArtefact artefact);
		#endregion
		
		#region Query Methods
//		[OperationContract]
//		object CreateQuery(byte[] binary);
//		
////		[OperationContract]
////		int QueryCount(object queryId);
////		
////		[OperationContract]
////		TArtefact QueryResult(object queryId);
////		
		[OperationContract]
		TArtefact[] QueryResults(object queryId, int startIndex = 0, int count = -1);
//		
//		[OperationContract]
//		object QueryMethodCall(object queryId, string methodName);// MethodInfo method);
		
		[OperationContract]
		object QueryExecute(object query);		//byte[] binary);
		#endregion
		
		#region Get/Set default paging options
		[OperationContract]
		PagingOptions GetDefaultPagingOptions();
		
		[OperationContract]
		void SetDefaultPagingOptions(PagingOptions pagingOptions);
		#endregion
	}
}

