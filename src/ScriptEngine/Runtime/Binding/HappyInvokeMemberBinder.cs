/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using Happy.ScriptEngine.Resources;
using Happy.ScriptEngine.Runtime.Trackers;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Runtime;

namespace Happy.ScriptEngine.Runtime.Binding
{
	// HappyInvokeMemberBinder is used for general dotted expressions in function
	// calls for invoking members.
	class HappyInvokeMemberBinder : InvokeMemberBinder
	{
		readonly HappyLanguageContext _languageContext;

		public HappyInvokeMemberBinder(HappyLanguageContext context, string name, CallInfo callinfo)
			: base(name, true, callinfo)
		{ // true = ignoreCase
			_languageContext = context;
		}

		public override DynamicMetaObject FallbackInvokeMember(
			DynamicMetaObject target, DynamicMetaObject[] args,
			DynamicMetaObject errorSuggestion)
		{
			// First try COM binding.
			DynamicMetaObject result;
			if (ComBinder.TryBindInvokeMember(this, target, args, out result))
				return result;

			// Defer if any object has no value so that we evaulate their
			// Expressions and nest a CallSite for the InvokeMember.
			if (!target.HasValue || args.Any((a) => !a.HasValue))
			{
				var deferArgs = new DynamicMetaObject[args.Length + 1];
				for (int i = 0; i < args.Length; i++)
				{
					deferArgs[i + 1] = args[i];
				}
				deferArgs[0] = target;
				return Defer(deferArgs);
			}

			Func<DynamicMetaObject> getMethodNotFoundDMO = () => errorSuggestion ?? RuntimeHelpers.CreateThrow(target, args, RuntimeHelpers.GetTargetArgsRestrictions(target, args, false),
					typeof(MissingMethodException), String.Format(RuntimeMessages.PublicMethodDoesNotExist_Method_Type, this.Name, target.Value.GetType()));
			
            if (target.Value is HappyTypeTracker)
            {
                HappyTypeTracker typeTracker = (HappyTypeTracker) target.Value;
            	IHappyTracker found;
                
                if (typeTracker.FindMemberTracker(this.Name, out found))
                {
                	HappyMethodTracker tracker = found as HappyMethodTracker;
					if(tracker == null)
					{
						return errorSuggestion ??
									RuntimeHelpers.CreateThrow(
										target, args, RuntimeHelpers.GetTargetArgsRestrictions(target, args, false),
										typeof(MissingMemberException),
										String.Format(RuntimeMessages.MemberIsNotInvokable_MemberNameTypeName, found.Name, typeTracker.Name), args.ToString());
					}
                    return ResolveOverload(null, args, tracker.Methods, BindingRestrictions.GetInstanceRestriction(target.Expression, typeTracker));
                }

				return getMethodNotFoundDMO();
            }

        	MethodInfo[] methods = target.Value.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.Name == Name).ToArray();

			if(methods.Length != 0)
            	return this.ResolveOverload(target, args, methods);

        	return getMethodNotFoundDMO();
		}

		DynamicMetaObject ResolveOverload(DynamicMetaObject target, DynamicMetaObject[] args, MethodInfo[] methods, BindingRestrictions additionalRestrictions = null) 
		{
			OverloadResolver or;
			if (target != null)
			{
				List<DynamicMetaObject> tmp = args.ToList();
				tmp.Insert(0, target);
				args = tmp.ToArray();
				or = this._languageContext.OverloadResolverFactory.CreateOverloadResolver(args, new CallSignature(args.Length), CallTypes.None);
			}
			else
			{
				or = this._languageContext.OverloadResolverFactory.CreateOverloadResolver(args, new CallSignature(args.Length), CallTypes.None);
			}
			BindingTarget bt = or.ResolveOverload(this.Name, methods, NarrowingLevel.None, NarrowingLevel.All);
			BindingRestrictions restrictions = bt.RestrictedArguments != null ? bt.RestrictedArguments.GetAllRestrictions() : BindingRestrictions.Empty;

			if (!bt.Success)
			{
				foreach (DynamicMetaObject mo in args)
					restrictions = restrictions.Merge(BindingRestrictions.GetTypeRestriction(mo.Expression, mo.GetLimitType()));

				return DefaultBinder.MakeError(or.MakeInvalidParametersError(bt), restrictions, typeof(object));
			}

			if(additionalRestrictions != null)
				restrictions = restrictions.Merge(additionalRestrictions);

			Expression callExpression = bt.MakeExpression();

			if (callExpression.Type == typeof(void))
				callExpression = Expression.Block(new[] { callExpression, Expression.Constant(null) });
			else if (callExpression.Type != typeof(object))
				callExpression = Expression.Convert(callExpression, typeof(object));

			return new DynamicMetaObject(callExpression, restrictions);
		}

		public override DynamicMetaObject FallbackInvoke(DynamicMetaObject targetMO, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			var argexprs = new Expression[args.Length + 1];
			for (int i = 0; i < args.Length; i++)
			{
				argexprs[i + 1] = args[i].Expression;
			}
			argexprs[0] = targetMO.Expression;
			// Just "defer" since we have code in HappyInvokeBinder that knows
			// what to do, and typically this fallback is from a language like
			// Python that passes a DynamicMetaObject with HasValue == false.
			return new DynamicMetaObject(
				Expression.Dynamic(
					// This call site doesn't share any L2 caching
					// since we don't call GetInvokeBinder from BinderFactory.
					// We aren't plumbed to get the runtime instance here.
					new HappyInvokeBinder(new CallInfo(args.Length)),
					typeof(object), // ret type
					argexprs),
				// No new restrictions since HappyInvokeBinder will handle it.
				targetMO.Restrictions.Merge(
					BindingRestrictions.Combine(args)));
		}
	}
}

