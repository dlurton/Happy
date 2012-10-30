using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Runtime.Binding
{
	class HappyBinaryOperationBinder : BinaryOperationBinder
	{
		readonly ExpressionType[] _stringCompareTypes =
		{
			ExpressionType.LessThan, 
			ExpressionType.LessThanOrEqual, 
			ExpressionType.GreaterThan, 
			ExpressionType.GreaterThanOrEqual,
			ExpressionType.Equal,
			ExpressionType.NotEqual
		};

		public HappyBinaryOperationBinder(ExpressionType operation) : base(operation)
		{
		}

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
		{
			// Defer if any object has no value so that we evaulate their
			// Expressions and nest a CallSite for the InvokeMember.
			if (!target.HasValue || !arg.HasValue)
			{
				return Defer(target, arg);
			}
			var restrictions = target.Restrictions;
			if (target.Value == null)
				restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(target.Expression, Expression.Constant(null))));
			else
				restrictions = restrictions.Merge(BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));

			if (arg.Value == null)
				restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(arg.Expression, Expression.Constant(null))));
			else
				restrictions = restrictions.Merge(BindingRestrictions.GetTypeRestriction(arg.Expression, arg.LimitType));

			//We can compare against nulls for equality, but any other operation against a null target or null argument should result in a null reference exception
			if ((this.Operation != ExpressionType.Equal && this.Operation != ExpressionType.NotEqual) && (target.Value == null || arg.Value == null))
				return errorSuggestion ?? RuntimeHelpers.CreateThrow(target, null, restrictions, typeof(NullReferenceException));

			if (target.Value != null && target.Value.GetType() == typeof(string) && 
				arg.Value != null && arg.Value.GetType() == typeof(string))
			{
				if (Array.IndexOf(_stringCompareTypes, this.Operation) >= 0)
				{
					MethodInfo compare = typeof (string).GetMethod("Compare", new[] {typeof (string), typeof (string)});

					Expression compareCall = Expression.Call(compare,
					                                         Expression.Convert(target.Expression, typeof (string)),
					                                         Expression.Convert(arg.Expression, typeof (string)));

					return
						new DynamicMetaObject(
							RuntimeHelpers.EnsureObjectResult(Expression.MakeBinary(this.Operation, compareCall, Expression.Constant(0))),
							restrictions);
				}
				if(this.Operation == ExpressionType.Add)
				{
					MethodInfo compare = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });

					Expression compareCall = Expression.Call(compare,
															 Expression.Convert(target.Expression, typeof(string)),
															 Expression.Convert(arg.Expression, typeof(string)));
					return
						new DynamicMetaObject(
							RuntimeHelpers.EnsureObjectResult(compareCall),
							restrictions);
				}
			}


			return new DynamicMetaObject(
				RuntimeHelpers.EnsureObjectResult(
					Expression.MakeBinary(
						this.Operation,
						Expression.Convert(target.Expression, target.LimitType),
						Expression.Convert(arg.Expression, arg.LimitType))),
						restrictions
				);
		}
	}
}