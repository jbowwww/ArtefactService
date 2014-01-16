using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Artefacts.Services
{
	/// <summary>
	/// Queryable.
	/// </summary>
	/// <remarks>
	///	-	TODO: A generic version of this class?
	/// 	-	TODO: Tidy up attributes (most/all are related to serialization), remove unnecessary
	/// </remarks>
	[Serializable]
	public class Queryable<TArtefact> :
		System.Linq.IQueryable<TArtefact>
		where TArtefact : Artefact
	{
		public class QueryResultEnumerator :
			IEnumerator<TArtefact>
		{
			private readonly Queryable<TArtefact> _queryable;
			internal int index;

			public QueryResultEnumerator(Queryable<TArtefact> queryable)
			{
				_queryable = queryable;
				Reset();
			}
			
			#region IEnumerator[Artefact] implementation
			public TArtefact Current {
				get { return index < 0 || index >= _queryable.TotalCount ? null : _queryable[index]; }
			}
			#endregion

			#region IEnumerator implementation
			public bool MoveNext()
			{
				return ++index < _queryable.TotalCount;
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
		
		#region Private fields
		[NonSerialized]
		private static PagingOptions _defaultPagingOptions = new PagingOptions() { StartIndex = 0, PageSize = 10 };
		
		[NonSerialized]
		private object _serverQueryId = null;
		
		[DataMember]
		private int[] _artefactIds;
		
		[NonSerialized]
		private TArtefact[] _artefacts;
		
		[NonSerialized]
		private int _count = -1;
		
		[NonSerialized]
		private PagingOptions _paging = null;
		
		[NonSerialized]
		private Expression _expression;
		
		[NonSerialized]
		private System.Linq.IQueryProvider _provider;
		#endregion
		
		#region Properties & Indexers
		public IRepository<TArtefact> Repository {
			get;
			private set;
		}

		[XmlIgnore]
		[IgnoreDataMember]
		public object ServerQueryId {
			get
			{
				return _serverQueryId != null ? _serverQueryId : _serverQueryId = Provider.Execute(Expression);
			}
		}
		
		[XmlIgnore]
		[IgnoreDataMember]
		public int TotalCount {
			get { return _count >= 0 ? _count : _count = Repository.QueryCount(ServerQueryId); }
		}
		
		public int LoadedCount {
			get;
			private set;
		}

		[XmlIgnore] 
		[IgnoreDataMember]
		public PagingOptions Paging {
			get
			{ return _paging != null ? _paging : _defaultPagingOptions; }
		}
		
		[XmlIgnore]
		[IgnoreDataMember]
		public int NumPages {
			get { return TotalCount == 0 || Paging.PageSize == 0 ? 0 : (int)Math.Ceiling((decimal)TotalCount / Paging.PageSize); }
		}
				
		[XmlIgnore]
		[IgnoreDataMember]
		public TArtefact this[int index] {
			get
			{
				if (_artefacts == null)
					_artefacts = new TArtefact[TotalCount];
				if (_artefacts[index] == null)
				{
					int pageSize = Paging.PageSize;
					int pageWithIndex = index / pageSize;
					int pageStartIndex = pageWithIndex * pageSize;
					Repository.QueryResults(ServerQueryId, pageStartIndex, pageSize).CopyTo(_artefacts, pageStartIndex);
					LoadedCount += pageSize;
				}
				return _artefacts[index];
			}
		}
		#endregion
		
		#region IQueryable implementation
		[XmlIgnore]
		[IgnoreDataMember]
		public Type ElementType {
			get;
			private set;
		}

		[XmlIgnore]
		[IgnoreDataMember]
		public Expression Expression {
			get { return _expression; }
			private set { _expression = value; }
		}
		
		[XmlIgnore]
		[IgnoreDataMember]
		public System.Linq.IQueryProvider Provider {
			get { return _provider; }
			private set { _provider = value; }
		}
		#endregion
		
		public Queryable(System.Linq.IQueryProvider provider, IRepository<TArtefact> repository, Expression expression = null)
		{
			ElementType = typeof(TArtefact);
//			Expression = expression != null ? expression : Expression.f(this);		//.Empty();
			Expression = expression;
			if (provider == null)
				throw new ArgumentNullException("provider");
			Provider = provider;
			if (repository == null)
				throw new ArgumentNullException("repository");
			Repository = repository;
		}
		
		#region IEnumerable & IEnumerable[Artefact] implementation
		public IEnumerator<TArtefact> GetEnumerator()
		{
			return new QueryResultEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this as IEnumerable<TArtefact>).GetEnumerator();
		}
		#endregion
	}
}
