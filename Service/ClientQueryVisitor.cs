using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;

namespace Artefacts.Service
{
	public class ClientQueryVisitor : ClientQueryVisitor<Artefact>
	{
		public ClientQueryVisitor(IArtefactService repository, IDictionary<object, IQueryable> queryCache)
			: base(repository, queryCache) { }
	}

	public class ClientQueryVisitor<TArtefact> : ExpressionVisitor where TArtefact : Artefact
	{
		private IDictionary<object, IQueryable> _queryCache;

		public IArtefactService Repository { get; private set; }
		
		public ClientQueryVisitor(IArtefactService repository, IDictionary<object, IQueryable> queryCache)
		{
			Repository = repository;
			_queryCache = queryCache;
		}
  
		private static Expression StripQuotes(Expression e)
		{
			while (e != null && e.NodeType == ExpressionType.Quote)
				e = ((UnaryExpression)e).Operand;
			return e;
		}

		public override Expression Visit(Expression exp)
		{
//			return exp != null ? base.Visit(StripQuotes(exp)) : null;
			return base.Visit(StripQuotes(exp));
		}
		
		protected override Expression VisitUnary(UnaryExpression u)
		{

			return base.VisitUnary((UnaryExpression)StripQuotes(u));
		}

//		protected override Expression VisitConstant(ConstantExpression c)
//		{
//			return base.VisitConstant(c);
//		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			Expression mExpression = Visit(m.Expression);
			if (mExpression != null && mExpression.NodeType == ExpressionType.Constant)
			{
				const BindingFlags bf = BindingFlags.GetField | BindingFlags.GetProperty
				                        | BindingFlags.Instance | BindingFlags.Static
				                        | BindingFlags.Public | BindingFlags.NonPublic;
				return Expression.Constant(
					m.Member.DeclaringType.InvokeMember(
					m.Member.Name, bf, null,
					(mExpression as ConstantExpression).Value,
					new object[] {}), m.Type);
			}
			return base.VisitMemberAccess(m);
		}

		/// <summary>
		/// Visits the method call.
		/// </summary>
		/// <returns>The method call.</returns>
		/// <param name="m">M.</param>
		/// <remarks>
		/// // This only doesn't work because my queyr provider executes it using a repository query method (QueryExecute)
		/// that has return type of object, and should only be used for scalar results. (It does not have any artefact KnownType's)
		/// for method calls like FirstOrDefault() that produce an Artefact, you
		/// will need to find some way of detecting that return value, and running the method call expression's argument[0] expression as a query, to
		/// get artefact id, then retrieve artefact using repository getbyid() ??
		/// </remarks>
		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			Expression mObject = Visit(m.Object);
			ReadOnlyCollection<Expression> mArguments = VisitExpressionList(m.Arguments);			
			MethodInfo mi = m.Method;
			ParameterInfo[] pi = mi.GetParameters();
			
			if (mObject != null && mObject.NodeType == ExpressionType.Constant
			 && mArguments.All<Expression>((arg) => arg.NodeType == ExpressionType.Constant))
				return Expression.Constant(m.Method.Invoke((mObject as ConstantExpression).Value,
					mArguments.Cast<ConstantExpression>().Select<ConstantExpression, object>((ce) => ce.Value).ToArray()));
			else if (pi.Length > 0 && (typeof(IEnumerable).IsAssignableFrom(pi[0].GetType()) || typeof(IQueryable).IsAssignableFrom(pi[0].GetType())))
			{
				object id = Repository.QueryPreload(Visit(m.Arguments[0]).ToBinary());
				int[] result = null;
				if (mi.Name.Equals("First") || mi.Name.Equals("FirstOrDefault")
				 || mi.Name.Equals("Single") || mi.Name.Equals("SingleOrDefault"))
					result = Repository.QueryResults(id, 0, 1);
				else if (mi.Name.Equals("Last") || mi.Name.Equals("LastOrDefault"))
					result = Repository.QueryResults(id, m.Arguments.Count() - 1, 1);

				// TODO: ElementAt()
				Artefact artefact =
					result != null && result.Length > 0 ?
						Repository.Get(result[0]) :
						(Artefact)Activator.CreateInstance(m.Arguments[0].Type.GetElementType());
				return Expression.Constant(artefact);
			}
			return base.VisitMethodCall(m);
		}
		
		protected override Expression VisitNewArray(NewArrayExpression na)
		{
			ReadOnlyCollection<Expression> naExpressions = VisitExpressionList(na.Expressions);
			if (naExpressions.All<Expression>((arg) => arg.NodeType == ExpressionType.Constant))
			{
				Array elements = Array.CreateInstance(na.Type.GetElementType(), naExpressions.Count);
				for (int i = 0; i < naExpressions.Count; i++)
					elements.SetValue(((ConstantExpression)naExpressions[i]).Value, i);
				return Expression.Constant(elements, na.Type);
			}
			return base.VisitNewArray(na);
		}
	}
}


//protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression c)
//{
//	if (c.Type.GetInterface("System.Linq.IQueryable") != null)// == typeof(IQueryable<Artefact>))// && c.Value == null)
//	{
//
//		return Expression.Constant(Repository.Artefacts);
//	}
//	return c;
//}
//		protected override Expression VisitConstant(ConstantExpression c)
//		{
//			// TODO: 99.999999% sure this entire method is obsolete. If the breakpoint below hasn't been hit
//			// byt he next time you read this comment again, scrap the method. Keep the class as it will
//			// be needed sooner or later
//			if (c.Type.GetInterface("IIdentifiableQueryable") != null)		// is IIdentifiableQueryable)
//			{
////				Expression exp = (c.Value as IIdentifiableQueryable);//.Id;
//				if (!_queryCache.ContainsKey(c))
//					throw new InvalidProgramException("IIdentifiableQueryable with expression=\"" + c + "\" should be in client side cache but it is not");
//				return _queryCache[c].Expression;
//			}
//			return c;
//		}

