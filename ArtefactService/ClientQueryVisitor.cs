using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;

namespace Artefacts.Service
{
	public class ClientQueryVisitor : ClientQueryVisitor<Artefact>
	{
		public ClientQueryVisitor(IRepository<Artefact> repository, IDictionary<object, IQueryable> queryCache)
			: base(repository, queryCache) { }
	}

	public class ClientQueryVisitor<TArtefact> : ExpressionVisitor where TArtefact : Artefact
	{
		private IDictionary<object, IQueryable> _queryCache;

		public IRepository<TArtefact> Repository { get; private set; }
		
		public ClientQueryVisitor(IRepository<TArtefact> repository, IDictionary<object, IQueryable> queryCache)
		{
			Repository = repository;
			_queryCache = queryCache;
		}
  
		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (c.Type.GetInterface("IIdentifiableQueryable") != null)		// is IIdentifiableQueryable)
			{
				object id = (c.Value as IIdentifiableQueryable).Id;
				if (!_queryCache.ContainsKey(id))
					throw new InvalidProgramException("IIdentifiableQueryable with Id=\"" + id + "\" should be in client side cache but it is not");
				return _queryCache[id].Expression;
			}
			return c;
		}
	}
}

//		private static Expression StripQuotes(Expression e)
//		{
//			while (e.NodeType == ExpressionType.Quote)
//				e = ((UnaryExpression)e).Operand;
//			return e;
//		}
//		protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression c)
//		{
//			if (c.Type.GetInterface("System.Linq.IQueryable") != null)// == typeof(IQueryable<Artefact>))// && c.Value == null)
//			{
//				
//				return Expression.Constant(Repository.Artefacts);
//			}
//			return c;
//		}
		
//		protected override Expression VisitMemberAccess(MemberExpression m)
//		{
//			const BindingFlags bf = BindingFlags.GetField | BindingFlags.GetProperty
//				| BindingFlags.Instance | BindingFlags.Static
//				| BindingFlags.Public | BindingFlags.NonPublic;
//			if (m.Expression.NodeType == ExpressionType.Constant)
//			{
//				return Expression.Constant(
//				m.Member.DeclaringType.InvokeMember(
//				m.Member.Name, bf, null,
//				(m.Expression as ConstantExpression).Value,
//				new object[] {}), m.Type);
//			}
//			return m;
//		}
