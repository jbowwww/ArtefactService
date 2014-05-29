using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Hql.Ast.ANTLR;

namespace Artefacts.Service
{
	public class ServerQueryVisitor : ExpressionVisitor
	{
		public ArtefactRepository Repository { get; private set; }
			
		public ServerQueryVisitor(ArtefactRepository repository)
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
			if (typeof(ArtefactRepository).IsAssignableFrom(p.Type) && p.Name.Equals("ArtefactRepository"))
				return Expression.Constant(Repository, typeof(ArtefactRepository));
			return p;
		}
	}
}

