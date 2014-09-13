using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Artefacts.Services
{
	/// <summary>
	/// Queryable.
	/// </summary>
	/// <exception cref='NullReferenceException'>
	/// Is thrown when there is an attempt to dereference a null object reference.
	/// </exception>
	/// <remarks>
	///	-	TODO: A generic version of this class?
	/// </remarks>
	[DataContract]
	public class Queryable : IQueryable<Artefact>
	{
		internal class QueryableEnumerator : IEnumerator<Artefact>
		{
			#region Private fields
			private readonly Queryable _queryResult;
			private int index;
			#endregion
			
			public QueryableEnumerator(Queryable queryResult)
			{
				_queryResult = queryResult;
				Reset();
			}
			
			#region IEnumerator[Artefact] implementation
			public Artefact Current {
				get { return index < 0 || index >= _queryResult.TotalCount ? null : _queryResult[index]; }
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
				get { return (object)((this as IEnumerator<Artefact>).Current); }
			}
			#endregion

			#region IDisposable implementation
			public void Dispose()
			{
				
			}
			#endregion
		}
		
		[DataMember]
		private int[] _artefactIds;
		
		private Artefact[] _artefacts;
		
		[DataMember]
		public int TotalCount { get; private set; }
		
		[DataMember]
		public int LoadedCount { get; private set; }
		
		[DataMember]
		public int PageSize;
		
		public int NumPages {
			get { return (int)Math.Ceiling((decimal)TotalCount / PageSize); }
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
		
		internal Queryable(int[] artefactIds, PagingOptions pagingOptions, Expression expression, IQueryProvider queryProvider)
		{
			_artefactIds = artefactIds.Clone();
			TotalCount = _artefactIds.Length;
			LoadedCount = 0;
			PageSize = pagingOptions.PageSize;
			Expression = expression;
			Provider = queryProvider;
		}
		
		#region IEnumerable[Artefact] implementation
		public IEnumerator<Artefact> GetEnumerator()
		{
			return new QueryableEnumerator(this);
		}
		#endregion

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this as IEnumerable<Artefact>).GetEnumerator();
		}
		#endregion

		#region IQueryable implementation
		public Type ElementType {
			get { return typeof(Artefact); }
		}

		public Expression Expression {
			get; private set;
		}

		public IQueryProvider Provider {
			get; private set;
		}
		#endregion
	}
}
