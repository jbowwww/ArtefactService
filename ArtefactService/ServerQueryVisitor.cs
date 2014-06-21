using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

public override Expression Visit(Expression exp)
		{
			Expression reducedExp, longExp = base.Visit(exp);
			reducedExp = longExp != null && longExp.CanReduce ? longExp.Reduce() : longExp;
			return reducedExp;
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

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			if (m.Member.DeclaringType.Namespace.ToLower().CompareTo("artefacts.service") == 0)	//.Equals(typeof(Repository)))// && m.Expression == null)
			{
				object value = null;
				object container = null;
				if (m.Expression != null)
				{
					Expression mExp = this.Visit(m.Expression);
					if (!(mExp is ConstantExpression))
						throw new ApplicationException("m.Expression should be null or a ConstantExpression (after processing with ServerQueryVisitor)");
					container = ((ConstantExpression)mExp).Value;
				}
				switch (m.Member.MemberType)
				{
					case MemberTypes.Field:
						value = ((FieldInfo)m.Member).GetValue(container);
						break;
					case MemberTypes.Property:
						value = ((PropertyInfo)m.Member).GetValue(container, null);		// TODO: Need to handle indexer properties? IndexExpression does not derive from MemberExpression
						break;
					default:
						throw new ApplicationException(string.Format("Unknown MemberType in MemberExpression: \"{0}\"", m.Member.MemberType.ToString()));
				}
				return Expression.Constant(value);
			}
			return base.VisitMemberAccess(m);
		}
	}
}

