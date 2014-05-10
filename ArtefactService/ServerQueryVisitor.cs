using System;
using System.Linq;
using System.Linq.Expressions;

namespace Artefacts.Services
{
	public class ServerQueryVisitor : ExpressionVisitor
	{
		public IRepository<Artefact> Repository { get; private set; }
			
		public ServerQueryVisitor(IRepository<Artefact> repository)
		{
			Repository = repository;
		}
		
//		protected override Expression VisitConstant(ConstantExpression c)
//		{
//			if (c.Type == typeof(IQueryable<Artefact>) && c.Value == null)
//			{
//				return Expression.Constant(Repository.Artefacts);				
//			}
//			return c;
//		}
	}
}

