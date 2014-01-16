using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Artefacts.Services
{
	[DataContract]
	public class QueryResult :
		IEnumerable<Artefact>, IQueryable<Artefact>
	{
		internal class QueryResultEnumerator :
			IEnumerator<Artefact>
		{
			private readonly QueryResult _queryResult;
			public int index;

			public QueryResultEnumerator(QueryResult queryResult)
			{
				_queryResult = queryResult;
				Reset();
			}
			
			#region IEnumerator[Artefact] implementation
			public Artefact Current {
				get
				{
					return index < 0 || index >= _queryResult.TotalCount ? null : _queryResult[index];
				}
			}
			#endregion

			#region IEnumerator implementation
			public bool MoveNext()
			{
				return ++index < _queryResult.TotalCount;
			}

			public void Reset()
			{
				index = -1;
			}

			object IEnumerator.Current {
				get
				{
					return (object)((this as IEnumerator<Artefact>).Current);
				}
			}
			#endregion

			#region IDisposable implementation
			public void Dispose()
			{
				
			}
			#endregion
		}
		
		public IArtefactRepository<Artefact> ArtefactRepository;
		
		[DataMember]
		private int[] _artefactIds;

		private Artefact[] _artefacts;
		
		[DataMember]
		public int TotalCount;
		
		[DataMember]
		public int LoadedCount { get; private set; }
		
		[DataMember]
		public int PageSize;
		
		public int NumPages { get { return (int)Math.Ceiling((decimal)TotalCount / PageSize); } }
		
		public QueryResult(IArtefactRepository<Artefact> artefactRepository, Expression expression = null)
		{
			ArtefactRepository = artefactRepository;
			Expression = expression;
		}
		
		internal void SetArtefactIds(int[] artefactIds)
		{
			_artefactIds = new int[artefactIds.Length];
			artefactIds.CopyTo(_artefactIds, 0);
		}
		
		public Artefact this[int index] {
			get
			{
				if (_artefacts == null)
				{
					if (_artefactIds == null)
						throw new NullReferenceException("_artefactIds == null");
					_artefacts = new Artefact[_artefactIds.Length];
				}
				if (_artefacts[index] != null)
					return _artefacts[index];
				_artefacts[index] = ArtefactRepository.GetById(_artefactIds[index]);
				LoadedCount++;
				return _artefacts[index];
			}
		}
		
		#region IEnumerable[Artefact] implementation
		public IEnumerator<Artefact> GetEnumerator()
		{
			return new QueryResultEnumerator(this);
		}
		#endregion

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)((this as IEnumerable<Artefact>).GetEnumerator());
		}
		#endregion

		#region IQueryable implementation
		public Type ElementType {
			get
			{
				return typeof(Artefact);
			}
		}

		public System.Linq.Expressions.Expression Expression {
			get; private set;
		}

		public IQueryProvider Provider {
			get
			{
				return QueryProvider.Singleton;
			}
		}
		#endregion
	}
}


//		#region IQueryable implementation
//		public Type ElementType { get { return typeof(Artefact); } }
//
//		public System.Linq.Expressions.Expression Expression { get; set; }
//
//		public IQueryProvider Provider { get; set; }
//		#endregion
