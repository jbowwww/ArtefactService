using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Xml.Serialization;

namespace Artefacts.Service
{
	/// <summary>
	/// Queryable.
	/// </summary>
	/// <remarks>
	/// Removed this to see if it would make things easier for IQueryProvider.CreateQuery<TElement>()
	/// 	//		where TArtefact : Artefact
	/// </remarks>
	public class Queryable<TArtefact> : IOrderedQueryable<TArtefact>
	{
		public class QueryableEnumerator :
			IEnumerator<TArtefact>
		{
			internal Queryable<TArtefact> _queryable;
			internal int index;

			public QueryableEnumerator(Queryable<TArtefact> queryable)
			{
				_queryable = queryable;
				Reset();
			}
			
			#region IEnumerator[Artefact] implementation
			public TArtefact Current {
//				get { return index < 0 || index >= _queryable.Count() ? null : _queryable[index]; }
				get
				{
					if (index < 0)
						throw new InvalidOperationException("Enumerator does not have a current item");
					return _queryable[index];
				}
			}
			#endregion

			#region IEnumerator implementation
			public bool MoveNext()
			{
				return ++index < _queryable.Count;
			}

			public void Reset()
			{
				index = -1;
			}

			object IEnumerator.Current {
				get { return (object)((this as IEnumerator<TArtefact>).Current); }
			}
			#endregion

			#region IDisposable implementation
			public void Dispose()
			{
				
			}
			#endregion
		}
	
		#region Static members
		public static PagingOptions DefaultPagingOptions {
			get
			{
				return new PagingOptions()
				{
				//					StartIndex = 0,
					PageSize = 10
				};
			}
		}
		#endregion
		
		#region Private fields
		private int _count = -1;
		private TArtefact[] _artefacts;
		private PagingOptions _paging;
		#endregion
		
		#region Properties & indexers
		/// <summary>
		/// Gets or sets the time created.
		/// </summary>
		/// <remarks>
		/// Queryables could even be artefacts themselves?? 
		/// </remarks>
		public DateTime TimeCreated { get; private set; }
		
		public DateTime TimeRetrieved { get; private set; }
		
		public IRepository<Artefact> Repository { get; protected set; }
		
		public Type ElementType { get { return typeof(TArtefact); } }

		public object QueryId { get { return Expression.Id(); } }				//.ToString(); } }
//			get { return _serverQueryId != null ? _serverQueryId : _serverQueryId = Provider.Execute(Expression); }
		
		public Expression Expression { get; private set; }

		public IQueryProvider Provider { get; private set; }
		
		public int Count { get { return _count >= 0 ? _count : _count = Repository.QueryCount(QueryId); } }
		
		public int LoadedCount { get; private set; }

		public PagingOptions Paging {
			get { return _paging != null ? _paging : DefaultPagingOptions; }
			private set { _paging = value; }
		}
		
		public int NumPages {
			get { return Count == 0 || Paging.PageSize == 0 ? 0 : (int)Math.Ceiling((decimal)Count / Paging.PageSize); }
		}
		
		public TArtefact this[int index] {
			get
			{
				if (_artefacts == null)
					_artefacts = new TArtefact[Count];
				if (_artefacts[index] == null)
				{
					int pageSize = Paging.PageSize;
					int pageWithIndex = index / pageSize;
					int pageStartIndex = pageWithIndex * pageSize;
					Repository.QueryResults(QueryId, pageStartIndex, pageSize).CopyTo(_artefacts, pageStartIndex);
					LoadedCount += pageSize;
				}
				return _artefacts[index];
			}
		}
		#endregion
		
		internal Queryable(IQueryProvider provider, IRepository<Artefact> repository, Expression expression)
		{
			TimeCreated = DateTime.Now;
			TimeRetrieved = DateTime.MinValue;
			Provider = provider;
			Repository = repository;
			Expression = expression;

						/*QueryId =*/ repository.CreateQuery(expression.ToBinary(new BinaryFormatter()));
		}
		
		#region IEnumerable & IEnumerable[Artefact] implementation
		public IEnumerator<TArtefact> GetEnumerator()
		{
			return new QueryableEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this as IEnumerable<TArtefact>).GetEnumerator();
		}
		#endregion
		
	}
}
