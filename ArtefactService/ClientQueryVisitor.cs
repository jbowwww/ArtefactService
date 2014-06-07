using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Reflection;

namespace Artefacts.Service
{
	public class ClientQueryVisitor : ClientQueryVisitor<Artefact>
	{
		public ClientQueryVisitor(IRepository repository, IDictionary<object, IQueryable> queryCache)
			: base(repository, queryCache) { }
	}

	public class ClientQueryVisitor<TArtefact> : ExpressionVisitor where TArtefact : Artefact
	{
		private IDictionary<object, IQueryable> _queryCache;

		public IRepository Repository { get; private set; }
		
		public ClientQueryVisitor(IRepository repository, IDictionary<object, IQueryable> queryCache)
		{
			Repository = repository;
			_queryCache = queryCache;
		}
  
		private static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
				e = ((UnaryExpression)e).Operand;
			return e;
		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant)
			{
				const BindingFlags bf = BindingFlags.GetField | BindingFlags.GetProperty
				                        | BindingFlags.Instance | BindingFlags.Static
				                        | BindingFlags.Public | BindingFlags.NonPublic;
				return Expression.Constant(
					m.Member.DeclaringType.InvokeMember(
					m.Member.Name, bf, null,
					(m.Expression as ConstantExpression).Value,
					new object[] {}), m.Type);
			}
			return m;
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

