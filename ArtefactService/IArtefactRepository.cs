using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ServiceModel;

using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

namespace Artefacts.Services
{
//	public interface IArtefactRepository : IArtefactRepository<Artefact> { }
	
	[ServiceContract]
//	[ServiceKnownType(typeof(NHibernate.Linq.NhQueryable<Artefact>))]
//	[ServiceKnownType(typeof(List<Artefact>))]
	[ServiceKnownType("GetArtefactTypes", typeof(Artefact))]
	public interface IArtefactRepository<TArtefact> : IRepository
		where TArtefact : Artefact
	{
		[OperationContract]
		int Add(TArtefact artefact);
		
		[OperationContract]
		int GetId(TArtefact artefact);
		
		[OperationContract]
		TArtefact GetById(int id);
		
		[OperationContract]
		PagingOptions GetDefaultPagingOptions();
		
		[OperationContract]
		void SetDefaultPagingOptions(PagingOptions pagingOptions);
			
		[OperationContract]
		List<TArtefact> GetAll();
		
		[OperationContract]
		TArtefact[] RunLinq(ExpressionNode exNode);

		[OperationContract]
		QueryResult RunQuery(Func<Artefact, bool> queryFunc, PagingOptions pagingOptions = null);

		[OperationContract]
		void Update(TArtefact artefact);
		
		[OperationContract]
		void Remove(TArtefact artefact);
	}
	
	public interface IRepository { }
}

