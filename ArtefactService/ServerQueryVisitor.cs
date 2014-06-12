using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using NHibernate.Hql.Ast.ANTLR;

namespace Artefacts.Service
{
	public class ServerQueryVisitor : ExpressionVisitor
	{
		public static bool IsRepositoryPlaceHolder(Expression e)
		{
			return e.NodeType == ExpressionType.Parameter
				&& typeof(IRepository).IsAssignableFrom(e.Type)
				&& ((ParameterExpression)e).Name.Equals("Repository");
		}

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
			if (ServerQueryVisitor.IsRepositoryPlaceHolder(p))
			{
				return Expression.Constant(Repository, typeof(Repository));
			}
//			if (p.NodeType == ExpressionType.Parameter && p.Name.Equals("Artefacts"))
//				return Expression.Constant(Repository.Artefacts);

			else
				return p;
		}

//		protected override Expression VisitMemberAccess(MemberExpression m)
//		{
//			if (m.Expression.NodeType == ExpressionType.Constant && m.Type.Equals(typeof(Repository)))
//			{
//				if (m.Member.MemberType == System.Reflection.MemberTypes.Property && m.Member.Name.Equals("Session"))
//					return Expression.Constant(Repository.Session);
//			}
//			return base.VisitMemberAccess(m);
//		}
//
//				protected override Expression VisitMethodCall(MethodCallExpression m)
//		{
//			if (m.Type.IsGenericType && m.Method.Name.Equals("OfType") && ServerQueryVisitor.IsRepositoryPlaceHolder(m.Arguments[0]))
//				return Expression.Constant(typeof(NHibernate.Linq.LinqExtensionMethods).GetMethods()
//					.First((mi) => mi.Name.Equals("Query") && mi.GetGenericArguments().Length == 1)
//					.MakeGenericMethod(m.Method.GetGenericArguments()[0]).Invoke(null, new object[] { Repository.Session }));
//			return base.VisitMethodCall(m);
//		}

//		protected override Expression VisitTypeIs(TypeBinaryExpression b)
//		{
//
//			return base.VisitTypeIs(b);
//
//		}
	}
}

