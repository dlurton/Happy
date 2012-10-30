using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Scripting.ComInterop;

namespace HappyTemplate.Runtime.Binding
{
	public class HappySetIndexBinder : SetIndexBinder
	{
		public HappySetIndexBinder(CallInfo callinfo)
			: base(callinfo)
		{
		}

		public override DynamicMetaObject FallbackSetIndex(
			DynamicMetaObject target, DynamicMetaObject[] indexes,
			DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			// First try COM binding.
			DynamicMetaObject result;
			if (ComBinder.TryBindSetIndex(this, target, indexes, value, out result))
			{
				return result;
			}
			// Defer if any object has no value so that we evaulate their
			// Expressions and nest a CallSite for the InvokeMember.
			if (!target.HasValue || indexes.Any((a) => !a.HasValue) || !value.HasValue)
			{
				var deferArgs = new DynamicMetaObject[indexes.Length + 2];
				for (int i = 0; i < indexes.Length; i++)
				{
					deferArgs[i + 1] = indexes[i];
				}
				deferArgs[0] = target;
				deferArgs[indexes.Length + 1] = value;
				return Defer(deferArgs);
			}
			// Find our own binding.
			Expression valueExpr = value.Expression;
			////we convert a value of TypeModel to Type.
			//if (value.LimitType == typeof(TypeModel))
			//{
			//    valueExpr = RuntimeHelpers.GetRuntimeTypeMoFromModel(value).Expression;
			//}
			//Debug.Assert(target.HasValue && target.LimitType != typeof(Array));
			Expression setIndexExpr;
			//if (target.LimitType == typeof(Cons))
			//{
			//    if (indexes.Length != 1)
			//    {
			//        return errorSuggestion ??
			//            RuntimeHelpers.CreateThrow(
			//                 target, indexes, BindingRestrictions.Empty,
			//                 typeof(InvalidOperationException),
			//                 "Indexing list takes single index.  " + "Got " + indexes);
			//    }
			//    // Call RuntimeHelper.SetConsElt
			//    List<Expression> args = new List<Expression>();
			//    // The first argument is the list
			//    args.Add(
			//        Expression.Convert(
			//            target.Expression,
			//            target.LimitType)
			//    );
			//    // The second argument is the index.
			//    args.Add(Expression.Convert(indexes[0].Expression,
			//                                indexes[0].LimitType));
			//    // The last argument is the value
			//    args.Add(Expression.Convert(valueExpr, typeof(object)));
			//    // BinderFactory helper returns value stored.
			//    setIndexExpr = Expression.Call(
			//        typeof(RuntimeHelpers),
			//        "SetConsElt",
			//        null,
			//        args.ToArray());
			//}
			//else
			{
				Expression indexingExpr = RuntimeHelpers.GetIndexingExpression(
					target, indexes);
				// Assign returns the stored value, so we're good for BinderFactory.
				setIndexExpr = Expression.Assign(indexingExpr, valueExpr);
			}

			BindingRestrictions restrictions =
				RuntimeHelpers.GetTargetArgsRestrictions(target, indexes, false);
			return new DynamicMetaObject(
				RuntimeHelpers.EnsureObjectResult(setIndexExpr),
				restrictions);

		}
	}
}