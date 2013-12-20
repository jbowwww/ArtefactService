using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;

using NHibernate;
using NHibernate.Criterion;

namespace Artefacts.Services
{
	/// <summary>
	/// I artefact repository.
	/// </summary>
	/// <remarks>
	/// Interface defines service implemented by <see cref="ArtefactRepository"/> and exposes to clients
	/// </remarks>
	[ServiceContract]
	[ServiceKnownType(typeof(NHibernate.Linq.NhQueryable<Artefact>))]
	public interface IArtefactRepository
	{
		[OperationContract]
		int AddArtefact(Artefact artefact);

		[OperationContract]
		[ServiceKnownType(typeof(List<Artefact>))]
		IList<Artefact> /*IQueryable<Artefact>*/ Query();

		[OperationContract]
		IEnumerable<Artefact> Get(IDetachedQuery query);

		[OperationContract]
		IEnumerable<Artefact> GetCr(DetachedCriteria criteria);

		[OperationContract]
		void Subscribe(ICreator creator);
	}
}

