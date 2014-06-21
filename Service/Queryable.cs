using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Artefacts.Service
{
	/// <summary>
	/// Queryable.
	/// </summary>
	/// <remarks>
	/// 30/5/14
	/// -	Goign to try making this class (partially) serializable (by datacontractserializer) so that the Queryable<>
	/// 	object itself can be used in parameters of repository method members. (operations)
	/// </remarks>
	public class Queryable<TArtefact> : IOrderedQueryable<TArtefact> where TArtefact : Artefact
	{
		/// <summary>
		/// Queryable enumerator
		/// </summary>
		public class QueryableEnumerator : IEnumerator<TArtefact>
		{
			#region Private fields
			internal Queryable<TArtefact> _queryable;
			internal int index;
			#endregion

			/// <summary>
			/// Initializes a new instance of the <see cref="Artefacts.Service.Queryable`2+QueryableEnumerator"/> class.
			/// </summary>
			/// <param name="queryable">Queryable.</param>
			internal QueryableEnumerator(Queryable<TArtefact> queryable)
			{
				_queryable = queryable;
				Reset();
			}

			/// <summary>
			/// Gets the current item
			/// </summary>
			/// <returns>The current <typeparamref name="TArtefact"/></returns>
			/// <remarks>IEnumerator[TArtefact] implementation</remarks>
			public TArtefact Current {
				get
				{
					if (index < 0)
						throw new InvalidOperationException("Enumerator does not have a current item");
					return (TArtefact)_queryable[index];
//					return null;
				}
			}

			/// <summary>
			/// Moves the next.
			/// </summary>
			/// <returns><c>true</c>, if next was moved, <c>false</c> otherwise.</returns>
			/// <remarks>IEnumerator implementation</remarks>
			public bool MoveNext()
			{
				return ++index < _queryable.Count;
			}

			/// <summary>
			/// Reset this instance.
			/// </summary>
			/// <remarks>IEnumerator implementation</remarks>
			public void Reset()
			{
				index = -1;
			}

			/// <summary>
			/// Gets the current.
			/// </summary>
			/// <remarks>IEnumerator implementation</remarks>
			object IEnumerator.Current {
				get { return (object)((this as IEnumerator<TArtefact>).Current); }
			}

			/// <summary>
			/// Releases all resource used by the <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/> object.
			/// </summary>
			public void Dispose()
			{

			}
		}

		#region Public fields & properties
		/// <summary>
		/// The time created.
		/// </summary>
		public DateTime TimeCreated {
			get; private set;
		}

		/// <summary>
		/// The time retrieved.
		/// </summary>
		public DateTime TimeRetrieved {
			get; private set;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is up to date.
		/// </summary>
		public bool IsUpToDate {
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the provider.
		/// </summary>
		public RepositoryClientProxy Repository {
			get; private set;
		}
		
		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <remarks><see cref="System.Linq.IQueryable"/> implementation</remarks>
		IQueryProvider IQueryable.Provider {
			get { return (IQueryProvider)Repository; }
		}

		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <remarks><see cref="System.Linq.IQueryable"/> implementation</remarks>
		public Type ElementType {
			get { return typeof(TArtefact); }
		}

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		/// <remarks>
		/// <see cref="System.Linq.IQueryable"/> implementation
		/// Set accessor is private so that property may only be set by <see cref="Queryable`1[TArtefact]"/>, which
		/// will only be done by the constructor. Could have used a readonly field but this way the set accessor can be
		/// used to automatically set the other related properties like <see cref="Queryable`1[TArtefact].Id"/>,
		/// <see cref="Queryable`1[TArtefact].AsString"/>, ..etc
		/// </remarks>
		public Expression Expression {
			get { return _expression; }// _translatedExpression; }
			private set
			{
				_expression = value;			//.ReduceAndCheck();
				Id = _expression.Id();
//				ExpressionAsString = _expression.ToString();
//				ExpressionAsNode = _expression.ToExpressionNode();
//				ExpressionAsBinary = _expression.ToBinary();
			}
		}
		private Expression _expression;

		/// <summary>
		/// Gets the <see cref="Queryable`1[TArtefact].Expression"/> identifier.
		/// </summary>
		public object Id {
			get; private set;
		}

		/// <summary>
		/// Gets the server identifier.
		/// </summary>
		public object ServerId {
			get; private set;
		}
		#region Removed/disabled/experimental represenations of Expression
		/// <summary>
		/// Gets <see cref="Queryable`1[TArtefact].Expression"/> as string.
		/// </summary>
//		public string ExpressionAsString {
//			get; private set;
//		}

		/// <summary>
		/// Gets <see cref="Queryable`1[TArtefact].Expression"/> as <see cref="Serialize.Linq.ExpressionNode"/> .
		/// </summary>
//		public ExpressionNode ExpressionAsNode {
//			get; private set;
//		}
		
		/// <summary>
		/// Gets <see cref="Queryable`1[TArtefact].Expression"/> as binary.
		/// </summary>
//		public byte[] ExpressionAsBinary {
//			get; private set;
//		}
		#endregion

		/// <summary>
		/// Gets the count.
		/// </summary>
		public int Count {
			get { return _count < 0 ? _count = this.Count() : _count; }
		}
		private int _count = -1;

		/// <summary>
		/// Gets the <see cref="Artefacts.Service.Queryable`1[TArtefact]"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public Artefact this[int index] {
			get
			{
				if (_resultIds == null)
				{
					_resultIds = Repository.Channel.QueryResults(ServerId);	//, pageStartIndex, Paging.PageSize);
					_results = new TArtefact[_resultIds.Length];
				}
				if (_results[index] == null)
				{
//					int pageStartIndex = index - index % Paging.PageSize;
//					_resultChunk.CopyTo(_results, pageStartIndex);
					_results[index] = Repository.Channel.GetById(_resultIds[index]);
				}
				return _results[index];
			}
		}
		private Artefact[] _results;
		private int[] _resultIds;

		/// <summary>
		/// The paging.
		/// </summary>
		public readonly PagingOptions Paging;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.Queryable"/> class.
		/// </summary>
		/// <param name="repository">Provider.</param>
		/// <param name="expression">Expression.</param>
		/// <remarks>
		/// 	-	When a queryable is created it has a known expression. Logically this should be sent to server
		/// 		ASAP regardless of whether queryable is about to be enumerated or not, so that server can start
		///			preloading it. BUT that doesn't mean the calling thread should have to block until the client has
		/// 		serialized the expression/queryable, the server has processed it and prdouced an identifier and sent it back.
		/// 		SO, AFTER you are reasonably satisfied that this is working properly, you could consider refactoring the
		/// 		code that sets the Id property (currently set in Expression set accessor)
		/// </remarks>
		public Queryable(RepositoryClientProxy repository, Expression expression)
		{
			if (repository == null)
				throw new ArgumentNullException("provider");
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (!expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "Should implement System.Collections.IEnumerable");
//			if (!typeof(TArtefact).IsAssignableFrom(expression.Type.GetElementType()))
//				throw new ArgumentOutOfRangeException("expression", expression, "Should have an element type assignable to " + typeof(TArtefact).FullName);
			TimeCreated = DateTime.Now;
			TimeRetrieved = DateTime.MinValue;	
			Repository = repository;
			Expression = expression;
			Id = expression.Id();
			// TODO: See method remarks
			ServerId = Repository.Channel.QueryPreload(expression.ToBinary());	//ToExpressionNode());
//			Debug.Assert(ServerId.GetType().Equals(typeof(int)) && Id.GetType().Equals(typeof(int)) && ((int)ServerId == (int)Id));
//			if (serverId != Id)
//				throw new Exception(string.Format("serverId != Id ({0} != {1})", serverId, Id));
			Paging = Repository.DefaultPagingOptions;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="Artefacts.Service.Queryable`1"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode()
		{
			return (int)Id;
		}

		/// <summary>
		/// Retrieve this instance
		/// </summary>
//		internal void Retrieve()
//		{
//			Count = this.Count<Artefact>();
//			TimeRetrieved = DateTime.Now;
//		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		/// <remarks><see cref="System.Collections.Generic.IEnumerable[Artefact]" /> implementation</remarks>
		public IEnumerator<TArtefact> GetEnumerator()
		{
//			Retrieve();			// Could call here or let it get called on demand by the Count property get method
//			Count = this.Count<Artefact>();
			TimeRetrieved = DateTime.Now;
			return new QueryableEnumerator(this);
		}
		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <remarks><see cref="System.Collections.IEnumerable" /> implementation</remarks>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}
	}
}
