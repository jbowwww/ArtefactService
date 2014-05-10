using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;

using NHibernate.Criterion;

using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

namespace Artefacts.Services
{
	[ServiceContract]
	[ServiceKnownType(typeof(Queryable<Artefact>))]
	public interface IRepository<TArtefact>
		where TArtefact : Artefact
	{
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
		[OperationContract]
		object CreateQuery(byte[] binary);
		
		[OperationContract]
		Queryable<Artefact> CreateQueryable(byte[] binary);
		
		[OperationContract]
		Queryable<Artefact> GetQueryable(object queryId);
		
		[OperationContract]
		int QueryCount(object queryId);
		
		[OperationContract]
		Artefact QueryResult(object queryId);
		
		[OperationContract]
		Artefact[] QueryResults(object queryId, int startIndex = 0, int count = -1);
		
		[OperationContract]
		object QueryExecute(byte[] binary);
		#endregion
		
		#region Collections/Enumerables/Queryables
		IQueryable<Artefact> Artefacts { get; }
		
		IDictionary<Type, IQueryable> Queryables { get; }
		#endregion
		
		#region Get/Set default paging options
		[OperationContract]
		PagingOptions GetDefaultPagingOptions();
		
		[OperationContract]
		void SetDefaultPagingOptions(PagingOptions pagingOptions);
		#endregion
	}
}

