using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Antlr.Runtime.Tree;
using System.Reflection;
using System.Globalization;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using Serialize.Linq.Serializers;

namespace Artefacts.Service
{
	/// <summary>
	/// Client query provider.
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class ClientQueryProvider<TArtefact> : IQueryProvider where TArtefact : Artefact
	{
		/// <summary>
		/// An expression visitor used in constructor
		/// </summary>
		private readonly ExpressionVisitor _expressionVisitor;

		/// <summary>
		/// The repository.
		/// </summary>
		public IRepository<TArtefact> Repository {
			get; private set;
		}

		/// <summary>
		/// Gets the query cache.
		/// </summary>
		/// <remarks>
		/// If you have trouble converting items to IQueryable, try deriving Queryable`1[TArtefact] from a new base class Queryable
		/// that has no type parameters and use that as the element type for this dictionary
		/// </remarks>
		protected IDictionary<object, IQueryable> _queryCache;

		/// <summary>
		/// Initializes a new instance of the <see cref="Artefacts.Service.ClientQueryProvider`1"/> class.
		/// </summary>
		/// <param name="repository">Repository.</param>
		/// <param name="visitor">Visitor.</param>
		public ClientQueryProvider(IRepository<TArtefact> repository, ExpressionVisitor visitor = null)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");
			Repository = repository;
			_queryCache = new ConcurrentDictionary<object, IQueryable>();
			_expressionVisitor = visitor ?? new ClientQueryVisitor<TArtefact>(Repository, _queryCache);	// Activator.CreateInstance<TExpressionVisitor>();
		}

		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression</param>
		/// <returns>The query</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		public IQueryable CreateQuery(Expression expression)
		{
			return (IQueryable)((IQueryProvider)this).CreateQuery<TArtefact>(expression);
		}

		/// <summary>
		/// Creates the query.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TElement">The 1st type parameter.</typeparam>
		/// <returns>The query.</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");
			if (!expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "Should implement System.Collections.IEnumerable");
			if (!typeof(Artefact).IsAssignableFrom(typeof(TElement)))
				throw new ArgumentOutOfRangeException("TElement", typeof(TElement), "Should be subclass of Artefact");
			if (_queryCache.ContainsKey(expression))
				return (IQueryable<TElement>)_queryCache[expression];
//			Expression newExpression = _expressionVisitor.Visit(expression);
			IQueryable<TElement> queryable = (IQueryable<TElement>)Activator.CreateInstance(
				typeof(Queryable<>).MakeGenericType(typeof(TElement)), this, _expressionVisitor.Visit(expression));
//				BindingFlags.NonPublic, null,
//				new object[] { this, expression },
//				CultureInfo.CurrentCulture);
			_queryCache[expression] = (IQueryable)queryable;
			return queryable;
			//return new Queryable<TArtefact>(this, expression);
		}

		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <returns>The query result</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		public object Execute(Expression expression)
		{
//			if (expression.IsEnumerable() && typeof(TArtefact).IsAssignableFrom(expression.Type) && _queryCache.ContainsKey(expression))
//				return _queryCache[expression];
			if (expression.IsEnumerable())
				throw new InvalidOperationException();
			Expression newExpression = _expressionVisitor.Visit(expression);
			ExpressionNode en = newExpression.ToExpressionNode();
//			Queryable.
//			object query = new JsonSerializer().Serialize<ExpressionNode>(en);		//.ToJson();
			return Repository.QueryExecute(en);			//query);			//_expressionVisitor.Visit(expression).ToJson());		//.ToBinary());	
		}

		/// <summary>
		/// Execute the specified expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		/// <returns>The query result</returns>
		/// <remarks>IQueryProvider implementation</remarks>
		TResult IQueryProvider.Execute<TResult>(Expression expression)
		{
			return (TResult)Execute(expression);
		}
	}
}

