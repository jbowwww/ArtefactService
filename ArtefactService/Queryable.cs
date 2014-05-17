using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Artefacts.Service
{
	/// <summary>
	/// Queryable.
	/// </summary>
	/// <remarks>
	/// 
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
				get { return Current; }
			}

			/// <summary>
			/// Releases all resource used by the <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/> object.
			/// </summary>
			/// <remarks>Call <see cref="Dispose"/> when you are finished using the
			/// <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/>. The <see cref="Dispose"/> method leaves the
			/// <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/> in an unusable state. After calling
			/// <see cref="Dispose"/>, you must release all references to the
			/// <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/> so the garbage collector can reclaim the memory
			/// that the <see cref="Artefacts.Service.Queryable`1+QueryableEnumerator"/> was occupying.</remarks>
			public void Dispose()
			{

			}
		}

		/// <summary>
		/// The query count method.
		/// </summary>
		private readonly MethodInfo QueryCountMethod =
			#region typeof().GetMethod(...)
			typeof(System.Linq.IQueryable<TArtefact>).GetMethod("Count",
				BindingFlags.Public | BindingFlags.Instance,// | BindingFlags.Static,
				null, CallingConventions.Any,
				new Type[] { typeof(System.Linq.IQueryable<TArtefact>) },
				new ParameterModifier[] { });
			#endregion

		#region Private fields & properties
		/// <summary>
		/// The _query provider.
		/// </summary>
		private readonly ClientQueryProvider<Artefact> _queryProvider;

		/// <summary>
		/// The expression originally supplied to constructor processed by <see cref="_expressionVisitor"/> 
		/// </summary>
		private readonly Expression _expression;

		/// <summary>
		/// The expression originally supplied to constructor, translated by <see cref="_expressionVisitor"/> into
		/// <see cref="_translatedExpression"/> and converted to an <see cref="Serialize.Linq.Nodes.ExpressionNode"/> 
		/// </summary>
		private readonly ExpressionNode _expressionNode;

		/// <summary>
		/// The _expression binary.
		/// </summary>
		private readonly byte[] _expressionBinary;

		/// <summary>
		/// <see cref="_expressionNode" /> as a string
		/// </summary>
		private readonly string _expressionString;

		/// <summary>
		/// The _expression JSO.
		/// </summary>
		private readonly string _expressionJSON;

		/// <summary>
		/// The _expression identifier.
		/// </summary>
		private readonly object _expressionId;

		/// <summary>
		/// The _count.
		/// </summary>
		private int _count;

		/// <summary>
		/// The _results.
		/// </summary>
		private Artefact[] _results;
		#endregion

		#region Public fields & properties
		/// <summary>
		/// The time created.
		/// </summary>
		public readonly DateTime TimeCreated;

		/// <summary>
		/// The time retrieved.
		/// </summary>
		public DateTime TimeRetrieved {
			get; protected set;
		}

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		/// <remarks><see cref="System.Linq.IQueryable"/> implementation</remarks>
		public Expression Expression {
			get { return _expression; }// _translatedExpression; }
		}

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		public object Id {
			get { return _expressionId; }
		}

		/// <summary>
		/// Gets the type of the element.
		/// </summary>
		/// <remarks><see cref="System.Linq.IQueryable"/> implementation</remarks>
		public Type ElementType {
			get { return typeof(TArtefact); }
		}

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <remarks><see cref="System.Linq.IQueryable"/> implementation</remarks>
		public IQueryProvider Provider {
			get { return (IQueryProvider)_queryProvider; }
		}

		/// <summary>
		/// Gets as string.
		/// </summary>
		public string AsString {
			get { return _expressionString; }
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		public int Count {
			get
			{
				if (!IsUpToDate)
					Retrieve();
				return _count;
			}
			protected set
			{
				_count = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is up to date.
		/// </summary>
		public bool IsUpToDate {
			get
			{
				return TimeRetrieved != DateTime.MinValue &&
					DateTime.Now.Subtract(TimeRetrieved) < ArtefactRepository.ArtefactUpdateAgeLimit;
			}
		}

		public TArtefact this[int index] {
			get
			{
				return this.Skip(index).Single();				//_queryProvider.Repository.QueryExecute(_expressionJSON);
			}
//				if (_results == null)
//				{
//					//	_results = new TArtefact[Count];
//					_results = _queryProvider.Repository.QueryResults(_expressionId);
//				}
//				return _results[index];
//			}

		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.Queryable"/> class.
		/// </summary>
		/// <param name="provider">Provider.</param>
		/// <param name="expression">Expression.</param>
		public Queryable(ClientQueryProvider<Artefact> provider, Expression expression)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (!expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "Should implement System.Collections.IEnumerable");
//			if (!typeof(Artefact).IsAssignableFrom(expression.Type.GetElementType()))
//				throw new ArgumentOutOfRangeException("expression", expression, "Should have an element type assignable to Artefact");
			TimeCreated = DateTime.Now;
			TimeRetrieved = DateTime.MinValue;
			_queryProvider = provider;
			_expression = expression;
			_expressionNode = _expression.ToExpressionNode();
			_expressionBinary = _expression.ToBinary();
			_expressionString = _expression.ToString();		//_expressionNode.ToString();
			_expressionJSON = _expression.ToJson();
//			_expressionId = _queryProvider.Repository.CreateQuery(_expressionBinary);
		}

		/// <summary>
		/// Retrieve this instance
		/// </summary>
		internal void Retrieve()
		{
			Count = this.Count();
			TimeRetrieved = DateTime.Now;
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		/// <remarks><see cref="System.Collections.Generic.IEnumerable[Artefact]" /> implementation</remarks>
		public IEnumerator<TArtefact> GetEnumerator()
		{
			Retrieve();			// Could call here or let it get called on demand by the Count property get method
			return new QueryableEnumerator(this);
		}
		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		/// <remarks><see cref="System.Collections.IEnumerable" /> implementation</remarks>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this as IEnumerable<TArtefact>).GetEnumerator();
		}
	}
}
