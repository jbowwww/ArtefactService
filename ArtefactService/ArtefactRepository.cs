using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WCFChannel = System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.Reflection;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Criterion;

namespace Artefacts.Services
{
	/// <summary>
	/// Artefact repository.
	/// ! TODO
	/// </summary>
	/// <remarks>
	/// This will manage all <see cref="Artefact"/>s, allowing CRUD operations from
	/// other parts of the system, and providing persistence and query functionality
	/// </remarks>
	public class ArtefactRepository : IArtefactRepository, IObserver<IArtefact>
	{
		public ArtefactRepository ()
		{

		}

		public int AddArtefact (Artefact artefact)
		{
			try
			{
				ISession session = ArtefactRepositorySessionFactory.Session;
				using (ITransaction trans = session.BeginTransaction())
				{
					session.SaveOrUpdate(artefact);
					trans.Commit();
				}
				return (int) session.GetIdentifier(artefact);
			}
			catch (Exception ex)
			{
				Console.WriteLine("\nArtefactRepository.AddArtefact exception: " + ex.ToString());
				throw new FaultException(WCFChannel.MessageFault.CreateFault(
					new FaultCode(ex.GetType().Name, ex.GetType().Namespace),
					new FaultReason(ex.Message), new object[] { ex, artefact }));
			}
		}

		public IList<Artefact> /*IQueryable<Artefact>*/ Query()
		{
			try
			{
				ISession session = ArtefactRepositorySessionFactory.Session;
				using (ITransaction trans = session.BeginTransaction())
				{
					IQueryable<Artefact> q = from a in session.Query<Artefact>() select a;
					return q.ToList();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("\nArtefactRepository.Query exception: " + ex.ToString());
				throw new FaultException(WCFChannel.MessageFault.CreateFault(
					new FaultCode(ex.GetType().Name, ex.GetType().Namespace),
					new FaultReason(ex.Message), new object[] { ex }));
			}
		}

		public IEnumerable<Artefact> Get(IDetachedQuery query)
		{
			try
			{
				ISession session = ArtefactRepositorySessionFactory.Session;
				using (ITransaction trans = session.BeginTransaction())
				{
					IQuery q = query.GetExecutableQuery(session);
					IList<Artefact> result = q.List<Artefact>();
					return result;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("\nArtefactRepository.AddArtefact exception: " + ex.ToString());
				throw new FaultException(WCFChannel.MessageFault.CreateFault(
					new FaultCode(ex.GetType().Name, ex.GetType().Namespace),
					new FaultReason(ex.Message), new object[] { ex, query }));
			}
		}

		public IEnumerable<Artefact> GetCr (DetachedCriteria criteria)
		{
			throw new NotImplementedException();
		}

		public void Subscribe(ICreator creator)
		{
			creator.Subscribe(this);
		}

		#region IObserver implementation
		public void OnCompleted ()
		{
			throw new System.NotImplementedException();
		}

		public void OnError (Exception error)
		{
			throw new System.NotImplementedException();
		}

		public void OnNext (IArtefact value)
		{
//			AddArtefact(value);
		}
		#endregion

	}
}

