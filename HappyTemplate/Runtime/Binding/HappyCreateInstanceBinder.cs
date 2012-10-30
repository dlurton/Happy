using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using HappyTemplate.Resources;
using HappyTemplate.Runtime.Trackers;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace HappyTemplate.Runtime.Binding
{
	class HappyCreateInstanceBinder : CreateInstanceBinder
	{
		private HappyLanguageContext _languageContext;
		public HappyCreateInstanceBinder(HappyLanguageContext languageContext, CallInfo callinfo)
			: base(callinfo)
		{
			_languageContext = languageContext;
		}

		public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			BindingRestrictions restrictions;
			if (target.Value is HappyTypeTracker)
			{
				HappyTypeTracker tracker = (HappyTypeTracker)target.Value;
				OverloadResolver or = _languageContext.OverloadResolverFactory.CreateOverloadResolver(args, new CallSignature(args.Length), CallTypes.None);

				if(tracker.Constructors.Length == 0)
					return errorSuggestion ??
						RuntimeHelpers.CreateThrow(target, args, BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value), 
							typeof(MissingMemberException), String.Format(RuntimeMessages.NoConstructorsFound_TypeName, tracker.FullName));

				BindingTarget bt = or.ResolveOverload(tracker.FullName, tracker.Constructors, NarrowingLevel.None, NarrowingLevel.All);

				restrictions = BindingRestrictions.Combine(args);
				if (!bt.Success)
				{
					restrictions = args.Aggregate(restrictions, (current, mo) => current.Merge(BindingRestrictions.GetTypeRestriction(mo.Expression, mo.GetLimitType())));

					return DefaultBinder.MakeError(or.MakeInvalidParametersError(bt), restrictions, typeof (object));
				}

				restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, tracker);
				restrictions.Merge(bt.RestrictedArguments.GetAllRestrictions());
				Expression callExpression = bt.MakeExpression();

				if (callExpression.Type == typeof (void))
					callExpression = Expression.Block(new[] {callExpression, Expression.Constant(null)});
				else if (callExpression.Type != typeof (object))
					callExpression = Expression.Convert(callExpression, typeof (object));

				return new DynamicMetaObject(callExpression, restrictions);
			}

			return errorSuggestion ??
				RuntimeHelpers.CreateThrow(target, args, null, typeof(MissingMemberException),
					RuntimeMessages.FirstArgumentToNewMustBeInstanceOfSystemType);

			//// Defer if any object has no value so that we evaulate their
			//// Expressions and nest a CallSite for the InvokeMember.
			//if (!target.HasValue || args.Any((a) => !a.HasValue))
			//{
			//    var deferArgs = new DynamicMetaObject[args.Length + 1];
			//    for (int i = 0; i < args.Length; i++)
			//    {
			//        deferArgs[i + 1] = args[i];
			//    }
			//    deferArgs[0] = target;
			//    return Defer(deferArgs);
			//}
			//// Make sure target actually contains a Type.
			//if (!typeof(Type).IsAssignableFrom(target.LimitType))
			//{
			//    return errorSuggestion ??
			//        RuntimeHelpers.CreateThrow(
			//            target, args, BindingRestrictions.Empty,
			//            typeof(InvalidOperationException),
			//            "Type object must be used when creating instance -- " +
			//                args.ToString());
			//}
			//var type = target.Value as Type;
			//Debug.Assert(type != null);
			//var constructors = type.GetConstructors();
			//// Get constructors with right arg counts.
			//var ctors = constructors.
			//    Where(c => c.GetParameters().Length == args.Length);
			//List<ConstructorInfo> res = new List<ConstructorInfo>();
			//foreach (var c in ctors)
			//{
			//    if (RuntimeHelpers.ParametersMatchArguments(c.GetParameters(),
			//        args))
			//    {
			//        res.Add(c);
			//    }
			//}
			//// We generate an instance restriction on the target since it is a
			//// Type and the constructor is associate with the actual Type instance.
			//var restrictions =
			//    RuntimeHelpers.GetTargetArgsRestrictions(
			//        target, args, true);
			//if (res.Count == 0)
			//{
			//    return errorSuggestion ??
			//        RuntimeHelpers.CreateThrow(
			//            target, args, restrictions,
			//            typeof(MissingMemberException),
			//            "Can't bind create instance -- " + args.ToString());
			//}
			//var ctorArgs =
			//    RuntimeHelpers.ConvertArguments(
			//        args, res[0].GetParameters());
			//return new DynamicMetaObject(
			//    // Creating an object, so don't need EnsureObjectResult.
			//    Expression.New(res[0], ctorArgs),
			//    restrictions);
		}
	}
}