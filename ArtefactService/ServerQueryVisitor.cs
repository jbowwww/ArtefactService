using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Hql.Ast.ANTLR;

namespace Artefacts.Service
{
	public class ServerQueryVisitor : ExpressionVisitor
	{
		public Repository Repository { get; private set; }
			
		public ServerQueryVisitor(Repository repository)
		{
			Repository = repository;
		}

		protected override Expression VisitUnary(UnaryExpression u)
		{
			object unExId = u.Id();
			if (u.Type.HasElementType && Repository.QueryCache.ContainsKey(unExId))
				return Expression.Constant(Repository.QueryCache[unExId]);
			return base.VisitUnary(u);
		}

		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (typeof(IRepository).IsAssignableFrom(p.Type) && p.Name.Equals("Repository"))
//			{
//				if ()
				return Expression.Constant(Repository, typeof(Repository));
//			}
//			if (p.NodeType == ExpressionType.Parameter && p.Name.Equals("Artefacts"))
//				return Expression.Constant(Repository.Artefacts);

			else
				return p;
		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
//			if (m.Expression.NodeType == ExpressionType.Constant && m.Type.Equals(typeof(Repository)))
//			{
				if (m.Member.MemberType == System.Reflection.MemberTypes.Property && m.Member.Name.Equals("Session"))
					return Expression.Constant(Repository.Session);
//			}
			return base.VisitMemberAccess(m);
		}
		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
//			return (m.Type.IsGenericType && m.Method.Name.CompareTo("OfType")
//				&& m.Object.Type.IsEnum && m.Object.Type.GetElementType().Equals(typeof(Artefact) && m.NodeType == ))
				return base.VisitMethodCall(m);
		}
		protected override Expression VisitTypeIs(TypeBinaryExpression b)
		{

			return base.VisitTypeIs(b);

		}
	}
}

