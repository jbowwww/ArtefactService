using System;
using System.Linq;
using System.Linq.Expressions;

namespace Artefacts.Service
{
	public class ServerQueryVisitor : ExpressionVisitor
	{
		public ArtefactRepository Repository { get; private set; }
			
		public ServerQueryVisitor(ArtefactRepository repository)
		{
			Repository = repository;
		}


		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (c.Type.Equals(typeof(object)))			//typeof(IQueryable).IsAssignableFrom(c.Type))
			{
				if (c.Value == null)
					return Expression.Constant(Repository.Artefacts);	
				return Expression.Constant(Repository.QueryCache[c.Value]);
			}
			return c;
		}
	}
}

