using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace Artefacts.Service
{
	/// <summary>
	/// Client query provider.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Is thrown when an argument passed to a method is invalid because it is outside the allowable range of values as
	/// specified by the method.
	/// </exception>
	/// <remarks>
	/// //	where TArtefact : Artefact
	/// </remarks>
	public class ClientQueryProvider<TArtefact> : IQueryProvider
	{
		public IRepository<TArtefact> Repository { get; private set; }
		
		public ExpressionVisitor QueryVisitor { get; private set; }

		public Dictionary<Expression, IQueryable> QueryCache { get; private set; }
		
		public BinaryFormatter BinaryFormatter { get; private set; }
		
		public ClientQueryProvider(IRepository<TArtefact> repository)
		{
			Repository = repository;
			QueryVisitor = new ClientQueryVisitor((IRepository<Artefact>)Repository);
			QueryCache = new Dictionary<Expression, IQueryable>();
			BinaryFormatter = new BinaryFormatter();
		}

		#region IQueryProvider implementation
		public IQueryable CreateQuery(Expression expression)
		{
			if (!expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "IQueryable IQueryProvider.CreateQuery: Expression should be IEnumerable");
			Expression newExpression = QueryVisitor.Visit(expression); 
						Queryable<TArtefact> query = new Queryable<TArtefact>(this, (IRepository<Artefact>)Repository, newExpression);
						object qId = Repository.CreateQuery(newExpression.ToBinary(BinaryFormatter));			// TODO: In future you may not want to wait for and return the queryId from server: it is only the expression as a string, and if you wait for it it will delay until the server has deserialized expression tree, which might be non-trivial if the expression is complex
//			if (query.QueryId != qId)
//				throw new InvalidOperationException("QueryId mismatch between client and server");
			return (IQueryable)query;
		}

				/// <summary>
				/// Creates the query.
				/// </summary>
				/// <returns>The query.</returns>
				/// <param name="expression">Expression.</param>
				/// <typeparam name="TElement">The 1st type parameter.</typeparam>
				/// <remarks>
				/// TODO: Doesn't want to serialize lambda exp[ressions (I think) - try using ExpressionNode again??
				/// </remarks>
		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
		{
						//return (IQueryable<TElement>)(CreateQuery(expression));
						if (!expression.IsEnumerable())
								throw new ArgumentOutOfRangeException("expression", expression, "IQueryable IQueryProvider.CreateQuery: Expression should be IEnumerable");
						Expression newExpression = QueryVisitor.Visit(expression); 
						Queryable<TElement> query = new Queryable<TElement>(this, (IRepository<Artefact>)Repository, newExpression);
						//object qId = Repository.CreateQuery(newExpression.ToBinary(BinaryFormatter));			// TODO: In future you may not want to wait for and return the queryId from server: it is only the expression as a string, and if you wait for it it will delay until the server has deserialized expression tree, which might be non-trivial if the expression is complex
//			if (query.QueryId != qId)
//				throw new InvalidOperationException("QueryId mismatch between client and server");
						return (System.Linq.IQueryable<TElement>) query;
		}

		public object Execute(Expression expression)
		{
			if (expression.IsEnumerable())
				throw new ArgumentOutOfRangeException("expression", expression, "IQueryable IQueryProvider.Execute: Expression should not be IEnumerable");
			Expression newExpression = QueryVisitor.Visit(expression); 
			return Repository.QueryExecute(newExpression.ToBinary(BinaryFormatter));	
		}

		TResult IQueryProvider.Execute<TResult>(Expression expression)
		{
			return (TResult)Execute(expression);
		}
		#endregion
	}
}

