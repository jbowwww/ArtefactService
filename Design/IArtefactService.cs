using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Artefacts.Services
{
	/// <summary>
	/// I artefact repository.
	/// </summary>
	/// <remarks>
	/// Interface defines service implemented by <see cref="ArtefactRepository"/> and exposes to clients
	/// </remarks>
	[ServiceContract]
	public interface IArtefactService
	{		
		[OperationContract]
		Uri GetRepositoryAddress<TArtefact>()
			where TArtefact : Artefact;
		
		[OperationContract]
		void AddRepository (IRepository repository);
			
		[OperationContract]
		void AddTypedRepository<TArtefact>(IArtefactRepository<TArtefact> repository)
			where TArtefact : Artefact;
		
//		[OperationContract]
//		Guid GetSessionId();
		
//		[OperationContract]
//		long GetSessionTimestamp();
		
		[OperationContract]
		void BeginTransaction();
		
		[OperationContract]
		void EndTransaction(bool commit);
	}
}
