using System;
using System.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime.Trackers;

using Microsoft.Scripting.ComInterop;

namespace HappyTemplate.Runtime.Binding
{	
	// HappyGetMemberBinder is used for general dotted expressions for fetching
	// members.
	class HappyGetMemberBinder : GetMemberBinder
	{
		readonly HappyLanguageContext _languageContext;
		public HappyGetMemberBinder(HappyLanguageContext languageContext, string name)
			: base(name, true)
		{
			_languageContext = languageContext;
		}

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			// First try COM binding.
			DynamicMetaObject result;
			if (ComBinder.TryBindGetMember(this, target, out result, true))
				return result;
			// Defer if any object has no value so that we evaulate their
			// Expressions and nest a CallSite for the InvokeMember.
			if (!target.HasValue)
			{
				System.Diagnostics.Debugger.Break();
				return Defer(target);
			}

			if(target.Value == null)
				return errorSuggestion ??
					RuntimeHelpers.CreateThrow(target, null, BindingRestrictions.GetExpressionRestriction(Expression.Equal(target.Expression, 
						Expression.Constant(null))), typeof(NullReferenceException));

            if(target.Value is HappyNamespaceTracker)
            {
            	HappyNamespaceTracker namespaceTracker = (HappyNamespaceTracker) target.Value;
            	IHappyTracker tracker;
				BindingRestrictions restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, namespaceTracker);
            	if (namespaceTracker.TryGetMember(this.Name, out tracker))
            		return new DynamicMetaObject(Expression.Constant(tracker), restrictions);

				return errorSuggestion ??
					RuntimeHelpers.CreateThrow(target, null, restrictions,  typeof(MissingMemberException), new object[] { namespaceTracker.FullName, this.Name });
            }

			if(target.Value is HappyTypeTracker)
			{
				IHappyTracker found;
				HappyTypeTracker typeTracker = (HappyTypeTracker) target.Value;
				BindingRestrictions restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, typeTracker);

				if (typeTracker.FindMemberTracker(this.Name, out found))
				{
					if(found is HappyMethodTracker)
						return RuntimeHelpers.CreateThrow(target, null, restrictions, typeof(MissingMemberException), String.Format(Resources.RuntimeMessages.CannotGetValueOfMethod_MethodName, this.Name));

					if (found is HappyPropertyOrFieldTracker)
					{
						var expr = Expression.MakeMemberAccess(null, ((HappyPropertyOrFieldTracker) found).MemberInfo);
						return new DynamicMetaObject(RuntimeHelpers.EnsureObjectResult(expr), restrictions);
					}

					if (found is HappyTypeTracker)
					{
						//var htt = (HappyTypeTracker)found;
						return new DynamicMetaObject(Expression.Constant(found), restrictions);
					}

					throw new UnhandledCaseException();
				}

				return errorSuggestion ??
                    RuntimeHelpers.CreateThrow(target, null, restrictions, 
						typeof(MissingMemberException), new object[] { typeTracker.FullName, this.Name });
			}

			MemberInfo[] infos = target.Value.GetType().FindMembers(MemberTypes.Field | MemberTypes.Property,
			                                                        BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance,
			                                                        (memberInfo, arg) => memberInfo.Name == (string)arg, this.Name);
			if(infos.Length > 0)
			{
				DebugAssert.IsFalse(infos.Length > 1, "Found more than 1 field or property named {0} on type {1}", this.Name, target.Value.GetType());
				var expr = RuntimeHelpers.EnsureObjectResult(
					Expression.MakeMemberAccess(
						Expression.Convert(target.Expression, target.Value.GetType()), infos[0]));
				return new DynamicMetaObject(expr, BindingRestrictions.GetTypeRestriction(target.Expression, target.Value.GetType()));
			}

			DynamicMetaObject dmo = _languageContext.Binder.GetMember(this.Name, target, _languageContext.OverloadResolverFactory,
				HappyLanguageContext.BinderNoThrow, errorSuggestion);

			return dmo;
		}
	}
}