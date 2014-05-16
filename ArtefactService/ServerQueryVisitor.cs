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


		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (typeof(ArtefactRepository).IsAssignableFrom(p.Type) && p.Name.Equals("ArtefactRepository"))
				return Expression.Constant(Repository, typeof(ArtefactRepository));
			return p;
		}
	}
}

