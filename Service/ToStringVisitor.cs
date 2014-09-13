using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;
using NHibernate.Impl;
using NHibernate;

namespace Artefacts.Service
{
	public class ToStringVisitor : ExpressionVisitor
	{
	
		/// <summary>
		/// Visitation method stack updater.
		/// </summary>
		/// <exception cref="ArgumentNullException">Is thrown when an argument passed to a method is invalid because it is <see langword="null" /></exception>
		/// <remarks>
		/// Used by visitation methods in <see cref="ExpressionVisitor"/> (inside <see langword="using"/> statements that
		/// wrap the method body) to push expressions onto the stack at each method call, and automatically pop them back
		/// off when returning from the method
		///</remarks>
		public class VisitationMethodStackUpdater : IDisposable
		{
			private Stack<Expression> _stack;
			
			public VisitationMethodStackUpdater(Stack<Expression> stack, Expression pushExpression)
			{
				if (stack == null)
					throw new ArgumentNullException("stack");
				if (pushExpression == null)
					throw new ArgumentNullException("pushExpression");				
				_stack = stack;
				_stack.Push(pushExpression);
			}
			
			public void Dispose()
			{
				_stack.Pop();
			}
		}
		
		private Stack<Expression> _visitStack;
		
		private StringBuilder  _sb;

		public string FormattedString { get { return _sb.ToString(); } }
		
		public ToStringVisitor()
		{
			
		}
		
		public string MakeString(Expression exp)
		{
			_visitStack = new Stack<Expression>();
			_sb = new StringBuilder(128);
			Visit(exp);
			return _sb.ToString();
		}
		
		protected override Expression VisitUnary(UnaryExpression u)
		{
			using (IDisposable vmsu = new VisitationMethodStackUpdater(_visitStack, u))
			{
				switch (u.NodeType)
				{
					case ExpressionType.ArrayLength:
						Visit(u.Operand);
						_sb.Append(".Length");
						break;
					case ExpressionType.Convert:
					case ExpressionType.ConvertChecked:
						_sb.AppendFormat("(({0})", u.Type.FullName);
						Visit(u.Operand);
						_sb.Append(")");
						break;
					case ExpressionType.Negate:
					case ExpressionType.NegateChecked:
						_sb.Append("(-");
						Visit(u.Operand);
						_sb.Append(")");
						break;
					case ExpressionType.Not:
						_sb.Append("(!");
						Visit(u.Operand);
						_sb.Append(")");
						break;
					case ExpressionType.Quote:
						Visit(u.Operand);
						break;
					case ExpressionType.TypeAs:
						_sb.Append("(");
						Visit(u.Operand);
						_sb.AppendFormat(" as {0}", u.Type.FullName);
						break;
					case ExpressionType.UnaryPlus:
						_sb.Append("(+");
						Visit(u.Operand);
						_sb.Append(")");
						break;
					default:
						throw new ApplicationException(string.Format("Unsupported  UnaryExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", u.NodeType));
				}
				return u;
			}
		}
		
		protected override Expression VisitBinary(BinaryExpression b)
		{
			using (VisitationMethodStackUpdater vmsu = new VisitationMethodStackUpdater(_visitStack, b))
			{
				switch (b.NodeType)
				{
					case ExpressionType.Add:
					case ExpressionType.AddChecked:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" + ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Divide:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" / ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Modulo:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" % ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Multiply:
					case ExpressionType.MultiplyChecked:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" * ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Power:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" ^^ ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Subtract:
					case ExpressionType.SubtractChecked:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" - ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.And:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" & ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Or:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" | ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.ExclusiveOr:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" ^ ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.LeftShift:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" << ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.RightShift:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" >> ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.AndAlso:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" && ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.OrElse:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" || ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Equal:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" == ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.NotEqual:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" != ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.GreaterThanOrEqual:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" >= ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.GreaterThan:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" > ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.LessThanOrEqual:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" <= ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.LessThan:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" < ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.Coalesce:
						_sb.Append("(");
						Visit(b.Left);
						_sb.Append(" = ");
						Visit(b.Right);
						_sb.Append(")");
						break;
					case ExpressionType.ArrayIndex:
						Visit(b.Left);
						_sb.Append("[");
						Visit(b.Right);
						_sb.Append("])");
						break;					
					default:
						throw new ApplicationException(string.Format("Unsupported  UnaryExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", b.NodeType));
				}
			}
			return b;
		}
		
		protected override Expression VisitTypeIs(TypeBinaryExpression b)
		{
			using (VisitationMethodStackUpdater vmsu = new VisitationMethodStackUpdater(_visitStack, b))
			{
				switch (b.NodeType)
				{
					case ExpressionType.TypeIs:
						_sb.Append("(");
						Visit(b.Expression);
						_sb.AppendFormat(" is typeof({0}))", b.TypeOperand.FullName);
						break;
					default:
						throw new ApplicationException(string.Format("Unsupported  UnaryExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", b.NodeType));	
				}
			}
			return b;
		}
		
		protected override Expression VisitConstant(ConstantExpression c)
		{
			_sb.AppendFormat("value({0})", c.Value);
			return c;
		}
		
		protected override Expression VisitConditional(ConditionalExpression c)
		{
			switch (c.NodeType)
			{
				case ExpressionType.Conditional:
					_sb.Append("(");
					Visit(c.Test);
					_sb.Append(" ? ");
					Visit(c.IfTrue);
					_sb.Append(":");
					Visit(c.IfFalse);
					_sb.Append(")");
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  UnaryExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", c.NodeType));
			}
			return c;
		}
		
		protected override Expression VisitParameter(ParameterExpression p)
		{
			switch (p.NodeType)
			{
				case ExpressionType.Parameter:
					_sb.Append(p.IsByRef ? string.Concat("ref ", p.Name) : p.Name);
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  UnaryExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", p.NodeType));
			}
			return p;
		}
		
		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			switch (m.NodeType)
			{
				case ExpressionType.MemberAccess:
					if (m.Expression != null)
						Visit(m.Expression);
					else
						_sb.Append(m.Member.DeclaringType.FullName);
					_sb.AppendFormat(".{0}", m.Member.Name);
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  MemberExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", m.NodeType));
			}
			return m;
		}
		
		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			switch (m.NodeType)
			{
				case ExpressionType.Call:
					if (m.Arguments.Count == 1 && m.Arguments[0].NodeType == ExpressionType.MemberAccess
			 		 &&	typeof(ISession).IsAssignableFrom(m.Arguments[0].Type) && m.Method.Name == "Query"
			  		 &&	m.Method.IsStatic && m.Method.IsGenericMethod
			 		 &&	typeof(Artefact).IsAssignableFrom(m.Method.GetGenericArguments()[0]))
			 			_sb.AppendFormat( "Query<{1}>()", m.Method.Name, m.Method.GetGenericArguments()[0].FullName);
					else
					{
						bool isExtensionMethod = m.Method.IsDefined(typeof(ExtensionAttribute), false);
						// m.Method.IsDefined(typeof(ext m.Method.IsStatic && m.Method.CallingConvention.HasFlag(System.Reflection.CallingConventions.HasThis);
						if (m.Object != null)
							Visit(m.Object);
						else if (isExtensionMethod)
							Visit(m.Arguments[0]);
						else if (m.Method.IsStatic)
							_sb.Append(m.Method.DeclaringType.FullName);
						else
							throw new ApplicationException(string.Format("MethodCallExpression has null instance for a non-static method \"{0}.{1}\"", m.Method.DeclaringType.FullName, m.Method.Name));
						if (m.Method.IsGenericMethod)
							_sb.AppendFormat( ".{0}<{1}>", m.Method.Name, string.Join(", ", m.Method.GetGenericArguments().Select<Type, string>((T) => T.FullName)));
						else
							_sb.AppendFormat( ".{0}", m.Method.Name);
						_sb.Append("(");
	//					VisitExpressionList(isExtensionMethod ? m.Arguments.Skip(1).ToList().AsReadOnly() : m.Arguments);
						foreach(Expression e in isExtensionMethod ? m.Arguments.Skip(1).ToList().AsReadOnly() : m.Arguments)
		{
							Visit(e);
							_sb.Append(", ");
		}
						if (m.Arguments.Count > (isExtensionMethod ? 1 : 0))
	//					{
	//						if (!_sb[_sb.Length - 2 
							_sb.Remove(_sb.Length - 2, 2);
	//					}
						_sb.Append(")");
					}
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  MethodCallExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", m.NodeType));
			}
			return m;
		}
		
		protected override Expression VisitLambda(LambdaExpression lambda)
		{
			switch (lambda.NodeType)
			{
				case ExpressionType.Lambda:
//					if (!string.IsNullOrWhiteSpace(lambda.Name))
//						_sb.Append(lambda.Name);
					_sb.Append("(");
//					VisitExpressionList(lambda.Parameters);
foreach(ParameterExpression pe in lambda.Parameters)
	{
						Visit(pe);
						_sb.Append(", ");
	}
					if (lambda.Parameters.Count > 0)
//					{
//						if (!_sb[_sb.Length - 2 
						_sb.Remove(_sb.Length - 2, 2);
//					}
					_sb.Append(") => ");
					Visit(lambda.Body);
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  LambdaExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", lambda.NodeType));
			}
			return lambda;
		}
		
		protected override NewExpression VisitNew(NewExpression nex)
		{
			switch (nex.NodeType)
			{
				case ExpressionType.New:
					_sb.AppendFormat("new {0}(", nex.Constructor.DeclaringType.FullName);
					VisitExpressionList(nex.Arguments);
					if (nex.Members != null && nex.Members.Count > 0)
						throw new ApplicationException(string.Format("Unsupported  NewExpression with non-empty \"Members\""));
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  NewExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", nex.NodeType));
			}
			return nex;
		}
		
		protected override Expression VisitMemberInit(MemberInitExpression init)
		{
			switch (init.NodeType)
			{
				case ExpressionType.MemberInit:
					Visit(init.NewExpression);
					if (init.Bindings != null && init.Bindings.Count > 0)
					{
						_sb.Append(" { ");
						VisitBindingList(init.Bindings);
						_sb.Append(" }");
					}
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  MemberInitExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", init.NodeType));
			}
			return init;
		}
		
		protected override Expression VisitListInit(ListInitExpression init)
		{
			throw new NotImplementedException();
//			switch (init.NodeType)
//			{
//				case ExpressionType.ListInit:
//					Visit(init.NewExpression);
//					
//					break;		
//				default:
//						throw new ApplicationException(string.Format("Unsupported  ListInitExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", b.NodeType));
//			}
//			return init;
		}
		
		protected override Expression VisitNewArray(NewArrayExpression na)
		{
			switch (na.NodeType)
			{
				case ExpressionType.MemberAccess:
					_sb.AppendFormat("new {0}[] { ", na.Type.FullName);
					VisitExpressionList(na.Expressions);
					_sb.Append(" }");
					break;		
				default:
						throw new ApplicationException(string.Format("Unsupported  NewArrayExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", na.NodeType));
			}
			return na;
		}
		
		protected override Expression VisitInvocation(InvocationExpression iv)
		{
			switch (iv.NodeType)
			{
				case ExpressionType.Invoke:
					_sb.AppendFormat("Invoke(");
					Visit(iv.Expression);
					_sb.Append(", ");
					VisitExpressionList(iv.Arguments);
					_sb.Append(")");
					break;
				default:
						throw new ApplicationException(string.Format("Unsupported  InvocationExpression type (shouldn't happen: All ExpressionTypes are handled) \"{0}\"", iv.NodeType));
			}
			return iv;
		}
		
		protected override MemberBinding VisitBinding(MemberBinding original)
		{
			throw new NotImplementedException();
			//return base.VisitBinding(original);
		}
		
		protected override ElementInit VisitElementInitializer(ElementInit initializer)
		{
			throw new NotImplementedException();
			//return base.VisitElementInitializer(initializer);
		}
		
		protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			List<Expression> c = new List<Expression>();
			foreach (Expression e in original)
				c.Add(Visit(e));
			return c.AsReadOnly();
//			throw new NotImplementedException();
			//return base.VisitExpressionList(original);
		}
		
		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			throw new NotImplementedException();
			//return base.VisitMemberAssignment(assignment);
		}
		
		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			throw new NotImplementedException();
			//return base.VisitMemberMemberBinding(binding);
		}
		
		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			throw new NotImplementedException();
			//return base.VisitMemberListBinding(binding);
		}
		
		protected override IEnumerable<MemberBinding> VisitBindingList(System.Collections.ObjectModel.ReadOnlyCollection<MemberBinding> original)
		{
			throw new NotImplementedException();
			//return base.VisitBindingList(original);
		}
		
		protected override IEnumerable<ElementInit> VisitElementInitializerList(System.Collections.ObjectModel.ReadOnlyCollection<ElementInit> original)
		{
			throw new NotImplementedException();
			//return base.VisitElementInitializerList(original);
		}
	}
}

