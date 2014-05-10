using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Artefacts.Services
{
	/// <summary>
	/// Expression visitor.
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	/// <remarks>
	/// TODO: I think you can get rid of this class, as it's actually in System.Linq.Expressions. I found the code on the net &
	/// modified it slightly but it looks virtually identical. Derive ClientExpressionVisitor(and others) same as you have already.
	/// </remarks>
	public abstract class ExpressionVisitor
	{
		/// <summary>
		/// Visitation method stack updater.
		/// </summary>
		/// <exception cref="ArgumentNullException">Is thrown when an argument passed to a method is invalid because it is <see langword="null" /></exception>
		/// <remarks>
		/// Used by visitation methods in <see cref="ExpressionVisitor"/> (inside <see langword="using"/> statements that
		/// wrap the method body) to push expressions onto the stack at each method call, and automatically pop them back
		/// off when returning from the method
		//</remarks>
//		public class VisitationMethodStackUpdater : IDisposable
//		{
//			private Stack<Expression> _stack;
//			
//			public VisitationMethodStackUpdater(Stack<Expression> stack, Expression pushExpression)
//			{
//				if (_stack == null)
//					throw new ArgumentNullException("stack");
//				if (pushExpression == null)
//					throw new ArgumentNullException("pushExpression");				
//				_stack = stack;
//				_stack.Push(pushExpression);
//			}
//			
//			public void Dispose()
//			{
//				_stack.Pop();
//			}
//		}
//		
//		public Stack<Expression> VisitStack = new Stack<Expression>();
		
		public virtual Expression Visit(Expression exp)
		{
			if (exp == null)
				return exp;
			switch (exp.NodeType)
			{
			case ExpressionType.Negate:
			case ExpressionType.NegateChecked:
			case ExpressionType.Not:
			case ExpressionType.Convert:
			case ExpressionType.ConvertChecked:
			case ExpressionType.ArrayLength:
			case ExpressionType.Quote:
			case ExpressionType.TypeAs:
				return this.VisitUnary((UnaryExpression)exp);
			case ExpressionType.Add:
			case ExpressionType.AddChecked:
			case ExpressionType.Subtract:
			case ExpressionType.SubtractChecked:
			case ExpressionType.Multiply:
			case ExpressionType.MultiplyChecked:
			case ExpressionType.Divide:
			case ExpressionType.Modulo:
			case ExpressionType.And:
			case ExpressionType.AndAlso:
			case ExpressionType.Or:
			case ExpressionType.OrElse:
			case ExpressionType.LessThan:
			case ExpressionType.LessThanOrEqual:
			case ExpressionType.GreaterThan:
			case ExpressionType.GreaterThanOrEqual:
			case ExpressionType.Equal:
			case ExpressionType.NotEqual:
			case ExpressionType.Coalesce:
			case ExpressionType.ArrayIndex:
			case ExpressionType.RightShift:
			case ExpressionType.LeftShift:
			case ExpressionType.ExclusiveOr:
				return this.VisitBinary((BinaryExpression)exp);
			case ExpressionType.TypeIs:
				return this.VisitTypeIs((TypeBinaryExpression)exp);
			case ExpressionType.Conditional:
				return this.VisitConditional((ConditionalExpression)exp);
			case ExpressionType.Constant:
				return this.VisitConstant((ConstantExpression)exp);
			case ExpressionType.Parameter:
				return this.VisitParameter((ParameterExpression)exp);
			case ExpressionType.MemberAccess:
				return this.VisitMemberAccess((MemberExpression)exp);
			case ExpressionType.Call:
				return this.VisitMethodCall((MethodCallExpression)exp);
			case ExpressionType.Lambda:
				return this.VisitLambda((LambdaExpression)exp);
			case ExpressionType.New:
				return this.VisitNew((NewExpression)exp);
			case ExpressionType.NewArrayInit:
			case ExpressionType.NewArrayBounds:
				return this.VisitNewArray((NewArrayExpression)exp);
			case ExpressionType.Invoke:
				return this.VisitInvocation((InvocationExpression)exp);
			case ExpressionType.MemberInit:
				return this.VisitMemberInit((MemberInitExpression)exp);
			case ExpressionType.ListInit:
				return this.VisitListInit((ListInitExpression)exp);
			default:
				throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
			}
		}

		#region Expression visitation methods
		protected virtual Expression VisitUnary(UnaryExpression u)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, u))
//			{
				Expression operand = this.Visit(u.Operand);
				if (operand != u.Operand)
					return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
				return u;
//			}
		}
 
		protected virtual Expression VisitBinary(BinaryExpression b)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, b))
//			{
				Expression left = this.Visit(b.Left);
				Expression right = this.Visit(b.Right);
				Expression conversion = this.Visit(b.Conversion);
				if (left != b.Left || right != b.Right || conversion != b.Conversion)
				{
					if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
						return Expression.Coalesce(left, right, conversion as LambdaExpression);
					else
						return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
				}
				return b;
//			}
		}
 
		protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, b))
//			{
				Expression expr = this.Visit(b.Expression);
				if (expr != b.Expression)
					return Expression.TypeIs(expr, b.TypeOperand);
				return b;
//			}
		}
 
		protected virtual Expression VisitConstant(ConstantExpression c)
		{
			return c;
		}
 
		protected virtual Expression VisitConditional(ConditionalExpression c)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, c))
//			{
				Expression test = this.Visit(c.Test);
				Expression ifTrue = this.Visit(c.IfTrue);
				Expression ifFalse = this.Visit(c.IfFalse);
				if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
					return Expression.Condition(test, ifTrue, ifFalse);
				return c;
//			}
		}
 
		protected virtual Expression VisitParameter(ParameterExpression p)
		{
			return p;
		}
 
		protected virtual Expression VisitMemberAccess(MemberExpression m)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, m))
//			{
				Expression exp = this.Visit(m.Expression);
				if (exp != m.Expression)
					return Expression.MakeMemberAccess(exp, m.Member);
				return m;
//			}
		}
 
		protected virtual Expression VisitMethodCall(MethodCallExpression m)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, m))
//			{
				Expression obj = this.Visit(m.Object);
				IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);
				if (obj != m.Object || args != m.Arguments)
					return Expression.Call(obj, m.Method, args);
				return m;
//			}
		}
 
		protected virtual Expression VisitLambda(LambdaExpression lambda)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, lambda))
//			{
				Expression body = this.Visit(lambda.Body);
				if (body != lambda.Body)
					return Expression.Lambda(lambda.Type, body, lambda.Parameters);
				return lambda;
//			}
		}
 
		protected virtual NewExpression VisitNew(NewExpression nex)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, nex))
//			{
				IEnumerable<Expression> args = this.VisitExpressionList(nex.Arguments);
				if (args != nex.Arguments)
				{
					if (nex.Members != null)
						return Expression.New(nex.Constructor, args, nex.Members);
					else
						return Expression.New(nex.Constructor, args);
				}
				return nex;
//			}
		}
 
		protected virtual Expression VisitMemberInit(MemberInitExpression init)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, init))
//			{
				NewExpression n = this.VisitNew(init.NewExpression);
				IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
				if (n != init.NewExpression || bindings != init.Bindings)
					return Expression.MemberInit(n, bindings);
				return init;
//			}
		}
 
		protected virtual Expression VisitListInit(ListInitExpression init)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, init))
//			{
				NewExpression n = this.VisitNew(init.NewExpression);
				IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
				if (n != init.NewExpression || initializers != init.Initializers)
					return Expression.ListInit(n, initializers);
				return init;
//			}
		}
 
		protected virtual Expression VisitNewArray(NewArrayExpression na)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, na))
//			{
				IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
				if (exprs != na.Expressions)
				{
					if (na.NodeType == ExpressionType.NewArrayInit)
						return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
					else
						return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
				}
				return na;
//			}
		}
 
		protected virtual Expression VisitInvocation(InvocationExpression iv)
		{
//			using (IDisposable vmsu = new VisitationMethodStackUpdater(VisitStack, iv))
//			{
				IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
				Expression expr = this.Visit(iv.Expression);
				if (args != iv.Arguments || expr != iv.Expression)
					return Expression.Invoke(expr, args);
				return iv;
//			}
		}
		#endregion
		
		#region Non-expression visitation methods
		protected virtual MemberBinding VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
			case MemberBindingType.Assignment:
				return this.VisitMemberAssignment((MemberAssignment)binding);
			case MemberBindingType.MemberBinding:
				return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
			case MemberBindingType.ListBinding:
				return this.VisitMemberListBinding((MemberListBinding)binding);
			default:
				throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
			}
		}
 
		protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
		{
			ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
			if (arguments != initializer.Arguments)
			{
				return Expression.ElementInit(initializer.AddMethod, arguments);
			}
			return initializer;
		}
 
		protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			List<Expression> list = null;
			for (int i = 0, n = original.Count; i < n; i++)
			{
				Expression p = this.Visit(original[i]);
				if (list != null)
				{
					list.Add(p);
				}
				else if (p != original[i])
				{
					list = new List<Expression>(n);
					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(p);
				}
			}
			if (list != null)
			{
				return list.AsReadOnly();
			}
			return original;
		}
 
		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			Expression e = this.Visit(assignment.Expression);
			if (e != assignment.Expression)
			{
				return Expression.Bind(assignment.Member, e);
			}
			return assignment;
		}
 
		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
			if (bindings != binding.Bindings)
			{
				return Expression.MemberBind(binding.Member, bindings);
			}
			return binding;
		}
 
		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
			if (initializers != binding.Initializers)
			{
				return Expression.ListBind(binding.Member, initializers);
			}
			return binding;
		}
 
		protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
		{
			List<MemberBinding> list = null;
			for (int i = 0, n = original.Count; i < n; i++)
			{
				MemberBinding b = this.VisitBinding(original[i]);
				if (list != null)
				{
					list.Add(b);
				}
				else if (b != original[i])
				{
					list = new List<MemberBinding>(n);
					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(b);
				}
			}
			if (list != null)
				return list;
			return original;
		}
 
		protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
		{
			List<ElementInit> list = null;
			for (int i = 0, n = original.Count; i < n; i++)
			{
				ElementInit init = this.VisitElementInitializer(original[i]);
				if (list != null)
				{
					list.Add(init);
				}
				else if (init != original[i])
				{
					list = new List<ElementInit>(n);
					for (int j = 0; j < i; j++)
					{
						list.Add(original[j]);
					}
					list.Add(init);
				}
			}
			if (list != null)
				return list;
			return original;
		}
		#endregion
	}
}

