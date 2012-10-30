using System;
using System.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime.Trackers;
using Microsoft.Scripting.ComInterop;

namespace HappyTemplate.Runtime.Binding
{	
	// HappySetMemberBinder is used for general dotted expressions for setting
	// members.
	//
	class HappySetMemberBinder : SetMemberBinder
	{
		private HappyLanguageContext _languageContext;

		public HappySetMemberBinder(HappyLanguageContext languageContext, string name)
			: base(name, true)
		{
			_languageContext = languageContext;
		}

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject targetMO, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			if (targetMO.Value is HappyTypeTracker)
			{
				IHappyTracker found;
				HappyTypeTracker typeTracker = (HappyTypeTracker)targetMO.Value;
				BindingRestrictions restrictions = BindingRestrictions.GetInstanceRestriction(targetMO.Expression, typeTracker);

				if (typeTracker.FindMemberTracker(this.Name, out found))
				{
					if (found is HappyMethodTracker)
						return RuntimeHelpers.CreateThrow(targetMO, null, restrictions, typeof(MissingMemberException), 
							String.Format(Resources.RuntimeMessages.CannotSetValueOfMethod_MethodName, this.Name));

					if (found is HappyPropertyOrFieldTracker)
					{
						//Expression valueExpr = RuntimeHelpers.EnsureObjectResult(targetMO.Expression);
						Expression valueExpr = value.Expression;
						Expression memberExpr = Expression.MakeMemberAccess(null, ((HappyPropertyOrFieldTracker) found).MemberInfo);
						//Expression memberExpr = Expression.PropertyOrField(null, ((HappyPropertyOrFieldTracker)found).Name);
						if(valueExpr.GetType() != memberExpr.Type)
							valueExpr = Expression.Convert(valueExpr, memberExpr.Type);

						Expression setExpr = RuntimeHelpers.EnsureObjectResult(Expression.Assign(memberExpr, valueExpr));
						return new DynamicMetaObject(setExpr, restrictions);
					}

					throw new UnhandledCaseException();
				}

				return errorSuggestion ??
					RuntimeHelpers.CreateThrow(targetMO, null, restrictions, typeof(MissingMemberException), new object[] { typeTracker.FullName, this.Name });
			}

			// First try COM binding.
			DynamicMetaObject result;
			if (ComBinder.TryBindSetMember(this, targetMO, value, out result))
			{
				return result;
			}
			// Defer if any object has no value so that we evaulate their
			// Expressions and nest a CallSite for the InvokeMember.
			if (!targetMO.HasValue) return Defer(targetMO);
			// Find our own binding.
			var flags = BindingFlags.IgnoreCase | BindingFlags.Static |
				BindingFlags.Instance | BindingFlags.Public;
			var members = targetMO.LimitType.GetMember(this.Name, flags);
			if (members.Length == 1)
			{
				MemberInfo mem = members[0];
				Expression val;
				// Should check for member domain type being Type and value being
				// TypeModel, similar to ConvertArguments, and building an
				// expression like GetRuntimeTypeMoFromModel.
				if (mem.MemberType == MemberTypes.Property)
					val = Expression.Convert(value.Expression,
						((PropertyInfo)mem).PropertyType);
				else if (mem.MemberType == MemberTypes.Field)
					val = Expression.Convert(value.Expression,
						((FieldInfo)mem).FieldType);
				else
					return (errorSuggestion ??
						RuntimeHelpers.CreateThrow(
							targetMO, null,
							BindingRestrictions.GetTypeRestriction(
								targetMO.Expression,
								targetMO.LimitType),
							typeof(InvalidOperationException),
							"BinderFactory only supports setting Properties and " +
								"fields at this time."));
				return new DynamicMetaObject(
					// Assign returns the stored value, so we're good for BinderFactory.
					RuntimeHelpers.EnsureObjectResult(
						Expression.Assign(
							Expression.MakeMemberAccess(
								Expression.Convert(targetMO.Expression,
									members[0].DeclaringType),
								members[0]),
							val)),
					// Don't need restriction test for name since this
					// rule is only used where binder is used, which is
					// only used in sites with this binder.Name.                    
					BindingRestrictions.GetTypeRestriction(targetMO.Expression,
						targetMO.LimitType));
			}
			else
			{
				return errorSuggestion ??
					RuntimeHelpers.CreateThrow(
						targetMO, null,
						BindingRestrictions.GetTypeRestriction(targetMO.Expression,
							targetMO.LimitType),
						typeof(MissingMemberException),
						"IDynObj member name conflict.");
			}
		}
	}
}