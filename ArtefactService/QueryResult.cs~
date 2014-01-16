using System;
using System.Collections.Generic;
using System.Linq;

using NHibernate.Linq;

namespace Artefacts.Services
{
	/// <summary>
	/// Query result.
	/// </summary>
	/// <typeparam name="TArtefact"></typeparam>
	public class QueryResult<TArtefact>
		where TArtefact : Artefact
	{
		#region Properties & Indexers
		public IQueryable<TArtefact> Queryable {
			get;
			private set;
		}
		
		public List<TArtefact> Results {
			get;
			private set;
		}
		
		public DateTime TimeCreated {
			get;
			private set;
		}
		
		public DateTime TimeRetrieved {
			get;
			private set;
		}
		
		public Type ElementType {
			get { return Queryable.ElementType; }
		}

		public System.Linq.Expressions.Expression Expression {
			get { return Queryable.Expression; }
		}

		public int Count {
			get { return Results.Count; }
		}
		
		public int[] ArtefactIds {
			get;
			private set;
		}
		
		public Artefact[] Artefacts {
			get;
			private set;
		}
		
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Services.QueryResult`1"/> class.
		/// </summary>
		/// <param name="queryable">Queryable</param>
		public QueryResult(IQueryable<TArtefact> queryable)
		{
			Queryable = queryable;
			TimeCreated = DateTime.Now;
			TimeRetrieved = DateTime.MinValue;

//			Count = (queryable as IEnumerable<Artefact>).ToList
			Results = queryable.ToList();
//			Count = Results.Count;
			
		}
		
		/// <summary>
		/// Sets the artefact identifiers.
		/// </summary>
		/// <param name="ids">Identifiers</param>
		public void SetArtefactIds(int[] ids)
		{
			if (ids == null)
				throw new ArgumentNullException("ids");
			if (ids.Length != Count)
				throw new ArgumentException("Array length does not equal this.Count", "ids");
			
//			Count = ids.Length;
			ArtefactIds = ids;
			Artefacts = new Artefact[Count];
		}
		
		/// <summary>
		/// Sets the artefact identifiers.
		/// </summary>
		/// <param name="artefact"></param>
		public void SetArtefact(Artefact artefact, int index)
		{
			Artefacts[index] = artefact;
		}
		
		/// <summary>
		/// Sets the artefact identifiers.
		/// </summary>
		/// <param name="artefacts"></param>
		public void SetArtefacts(Artefact[] artefacts, int index = 0)
		{
			artefacts.CopyTo(Artefacts, index);
		}
	}
}

