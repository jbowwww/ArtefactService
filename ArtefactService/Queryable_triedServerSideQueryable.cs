using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Xml.Serialization;

using Serialize.Linq;
using Serialize.Linq.Nodes;
using Serialize.Linq.Extensions;

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
		IQueryable<TArtefact>	, ISerializable
		where TArtefact : Artefact
	{
		public class QueryableStreamingContext
		{
			private StreamingContext _sc;
			
			public string ProviderTypeName { get; set; }
		
			public static implicit operator StreamingContext(QueryableStreamingContext qsc)
			{
				return qsc._sc;
			}
			
			public QueryableStreamingContext(IQueryProvider provider)
			{
				_sc = new StreamingContext(StreamingContextStates.Remoting, this);
				ProviderTypeName = provider.GetType().FullName;
			}
		}
		
		/// <summary>
		/// <see cref="Queryable"/> enumerator
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown when an operation cannot be performed</exception>
		public class QueryableEnumerator :
			IEnumerator<TArtefact>
		{
			#region Internal fields
			internal Queryable<TArtefact> _queryable;
			internal int index;
			#endregion
			
			public QueryableEnumerator(Queryable<TArtefact> queryable)
			{
				_queryable = queryable;
				Reset();
			}
			
			#region IEnumerator[Artefact] & IEnumerator implementation
			/// <summary>
			/// Gets the current enumerator's current <typeparamref name="TArtefact"/>
			/// </summary>
			/// <exception cref="InvalidOperationException">Is thrown when an operation cannot be performed</exception>
			public TArtefact Current {
//				get { return index < 0 || index >= _queryable.Count() ? null : _queryable[index]; }
				get
				{
					if (index < 0)
						throw new InvalidOperationException("Enumerator does not have a current item");
					return _queryable[index];
				}
			}
			
			/// <summary>
			/// Gets the current enumerator's current item
			/// </summary>
			object IEnumerator.Current {
				get { return (object)((this as IEnumerator<Artefact>).Current); }
			}

			/// <summary>
			/// Moves the enumerator to the next item
			/// </summary>
			/// <returns><c>true</c> if the enumerator contains another item (<see cref="Current"/> is readable after this method returns)</returns>
			public bool MoveNext()
			{
				return ++index < _queryable.Count;
			}
			
			/// <summary>
			/// Reset this enumerator
			/// </summary>
			public void Reset()
			{
				index = -1;
			}
			#endregion

			/// <summary>
			/// Releases all resource used by the <see cref="Artefacts.Services.Queryable`1.QueryableEnumerator"/> object.
			/// </summary>
			/// <remarks>
			/// IDisposable implementation
			/// Call <see cref="Dispose"/> when you are finished using the
			/// <see cref="Artefacts.Services.Queryable`1.QueryableEnumerator"/>. The <see cref="Dispose"/> method leaves the
			/// <see cref="Artefacts.Services.Queryable`1.QueryableEnumerator"/> in an unusable state. After calling
			/// <see cref="Dispose"/>, you must release all references to the
			/// <see cref="Artefacts.Services.Queryable`1.QueryableEnumerator"/> so the garbage collector can reclaim the memory
			/// that the <see cref="Artefacts.Services.Queryable`1.QueryableEnumerator"/> was occupying.
			/// </remarks>
			public void Dispose() { }
		}
		
		public static PagingOptions DefaultPagingOptions {
			get
			{
				return new PagingOptions()
				{
		//			StartIndex = 0, 
					PageSize = 10
				};
			}
		}
		
		#region Private fields
		private TArtefact[] _artefacts;
		
		private int _count = -1;
		
		private byte[] _expressionBinary;
		#endregion
		
		#region Properties & indexers
		public DateTime TimeCreated { get; private set; }
		
		public DateTime TimeRetrieved { get; private set; }

		public IRepository<TArtefact> Repository { get; private set; }
		
		public object ServerQueryId { get; private set; }
//			get { return _serverQueryId != null ? _serverQueryId : _serverQueryId = Provider.Execute(Expression); }
		
		public IQueryProvider Provider { get; private set; }

		public Expression Expression { get; internal set; }
		
		public Type ElementType { get; private set; }

		public int Count {
			get { return _count >= 0 ? _count : _count = Repository.QueryCount(ServerQueryId); }
			private set { _count = value; }
		}

		public int LoadedCount { get; private set; }

		public PagingOptions Paging { get; private set; }
		
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
					int pageWithIndex = index / Paging.PageSize;
					int pageStartIndex = pageWithIndex * Paging.PageSize;
					Repository.QueryResults(ServerQueryId, pageStartIndex, Paging.PageSize).CopyTo(_artefacts, pageStartIndex);
					LoadedCount += Paging.PageSize;
				}
				return _artefacts[index];
			}
		}
		#endregion

		#region ISerializable implementation
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("TimeCreated", TimeCreated);
			info.AddValue("TimeRetrieved", TimeRetrieved);
			info.AddValue("ServerQueryId", ServerQueryId);
			info.AddValue("ElementType", ElementType);
			info.AddValue("Count", Count);
			info.AddValue("Paging", Paging);
			info.AddValue("ExpressionBinary", _expressionBinary);
		}
		#endregion

//		[OnSerializing]
		public void OnSerializing(StreamingContext context)
		{
			;
		}
		
//		[OnDeserialized]
		public void OnDeserialized(StreamingContext context)
		{
			QueryableStreamingContext qc = (QueryableStreamingContext)context.Context;
			if (qc.ProviderTypeName == typeof(ClientQueryProvider).FullName)
			{
				// Going from client -> server
				// Is this going to be needed
			}
			else
			{
				// Server -> client
				Provider = ClientQueryProvider.Singleton;
				Repository = RepositoryClientProxy<TArtefact>.Singleton;
				LoadedCount = 0;
				Expression = _expressionBinary.FromBinary(ClientQueryProvider.Singleton.BinaryFormatter);
			}
		}
		
		protected Queryable(SerializationInfo info, StreamingContext context)
		{
			if (!this.GetType().IsAssignableFrom(info.ObjectType))
				throw new InvalidDataException(string.Format("{0} not assignable from {1}", this.GetType().FullName, info.ObjectType.FullName));
			TimeCreated = (DateTime)info.GetValue("TimeCreated", typeof(DateTime));
			TimeRetrieved = (DateTime)info.GetValue("TimeRetrieved", typeof(DateTime));
			ServerQueryId = info.GetValue("ServerQueryId", typeof(System.Object));
			ElementType = (Type)info.GetValue("ElementType", typeof(Type));
			Count = (int)info.GetValue("Count", typeof(int));
			Paging = (PagingOptions)info.GetValue("Paging", typeof(PagingOptions));
			_expressionBinary = (byte[])info.GetValue("ExpressionBinary", typeof(byte[]));
		}
		
		public Queryable(IRepository<TArtefact> repository, IQueryProvider provider, Expression expression, byte[] binary = null)
		{
			if (!typeof(IQueryable).IsAssignableFrom(expression.Type) && !typeof(IEnumerable).IsAssignableFrom(expression.Type))
				throw new ArgumentOutOfRangeException("expression", expression, "Expression type is not assignable to either System.Linq.IQueryable or System.Collections.IEnumerable");
			if (provider == null)
				throw new ArgumentNullException("provider");
			if (repository == null)
				throw new ArgumentNullException("repository");
		
			_expressionBinary = binary;
			TimeCreated = DateTime.Now;
			TimeRetrieved = DateTime.MinValue;			// TODO: Store retrieval time per page, per artefact?
			Repository = repository;
			Provider = provider;
			Expression = expression;
			ServerQueryId = Expression.ToString();
			Type iqueryable = Expression.Type.GetInterface("System.Linq.IQueryable`1");
			Type ienumerable = Expression.Type.GetInterface("System.Collections.Generic.IEnumerable`1");
			ElementType =
				iqueryable != null ? iqueryable.GetGenericArguments()[0] : 
				ienumerable != null ? ienumerable.GetGenericArguments()[0] :
				typeof(System.Object);
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
