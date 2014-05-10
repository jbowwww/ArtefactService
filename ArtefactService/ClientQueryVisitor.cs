using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

using System.Reflection;
namespace Artefacts.Services
{
	public class ClientQueryVisitor : ExpressionVisitor
	{
		public IRepository<Artefact> Repository { get; private set; }
		
		public ClientQueryVisitor(IRepository<Artefact> repository)
		{
			Repository = repository;
		}
  
//		private static Expression StripQuotes(Expression e)
//		{
//			while (e.NodeType == ExpressionType.Quote)
//				e = ((UnaryExpression)e).Operand;
//			return e;
//		}
		protected override System.Linq.Expressions.Expression VisitConstant(ConstantExpression c)
		{
			if (c.Type.GetInterface("System.Linq.IQueryable") != null)// == typeof(IQueryable<Artefact>))// && c.Value == null)
			{
				return Expression.Constant(Repository.Artefacts);
			}
			return c;
		}
		
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
	}
}