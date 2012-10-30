using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace HappyTemplate.Runtime
{
	class DynamicObjectHelpers
	{

		static private object _sentinel = new object();
		static internal object Sentinel { get { return _sentinel; } }

		internal static bool HasMember(IDynamicMetaObjectProvider o,
									   string name)
		{
			return (DynamicObjectHelpers.GetMember(o, name) !=
					DynamicObjectHelpers.Sentinel);

			//Alternative impl used when EOs had bug and didn't call fallback ...
			//var mo = o.GetMetaObject(Expression.Parameter(typeof(object), null));
			//foreach (string member in mo.GetDynamicMemberNames()) {
			//    if (string.Equals(member, name, StringComparison.OrdinalIgnoreCase)) {
			//        return true;
			//    }
			//}
			//return false;
		}

		static private Dictionary<string, CallSite<Func<CallSite, object, object>>>
			_getSites = new Dictionary<string, CallSite<Func<CallSite, object, object>>>();

		internal static object GetMember(IDynamicMetaObjectProvider o, string name)
		{
			CallSite<Func<CallSite, object, object>> site;
			if (!_getSites.TryGetValue(name, out site))
			{
				site = CallSite<Func<CallSite, object, object>>.Create(new DoHelpersGetMemberBinder(name));
				_getSites[name] = site;
			}
			return site.Target(site, o);
		}

		static private Dictionary<string, CallSite<Action<CallSite, object, object>>>
			_setSites = new Dictionary<string, CallSite<Action<CallSite, object, object>>>();

		internal static void SetMember(IDynamicMetaObjectProvider o, string name,
									   object value)
		{
			CallSite<Action<CallSite, object, object>> site;
			if (!_setSites.TryGetValue(name, out site))
			{
				site = CallSite<Action<CallSite, object, object>>.Create(new DoHelpersSetMemberBinder(name));
				_setSites[name] = site;
			}
			site.Target(site, o, value);
		}

		// CreateThrow is a convenience function for when binders cannot bind.
		// They need to return a DynamicMetaObject with appropriate restrictions
		// that throws.  Binders never just throw due to the protocol since
		// a binder or MO down the line may provide an implementation.
		//
		// It returns a DynamicMetaObject whose expr throws the exception, and 
		// ensures the expr's type is object to satisfy the CallSite return type
		// constraint.
		//
		// A couple of calls to CreateThrow already have the args and target
		// restrictions merged in, but BindingRestrictions.Merge doesn't add 
		// duplicates.
		//
		public static DynamicMetaObject CreateThrow
				(DynamicMetaObject target, DynamicMetaObject[] args,
				 BindingRestrictions moreTests,
				 Type exception, params object[] exceptionArgs)
		{
			Expression[] argExprs = null;
			Type[] argTypes = Type.EmptyTypes;
			int i;
			if (exceptionArgs != null)
			{
				i = exceptionArgs.Length;
				argExprs = new Expression[i];
				argTypes = new Type[i];
				i = 0;
				foreach (object o in exceptionArgs)
				{
					Expression e = Expression.Constant(o);
					argExprs[i] = e;
					argTypes[i] = e.Type;
					i += 1;
				}
			}
			ConstructorInfo constructor = exception.GetConstructor(argTypes);
			if (constructor == null)
			{
				throw new ArgumentException(
					"Type doesn't have constructor with a given signature");
			}
			return new DynamicMetaObject(
				Expression.Throw(
					Expression.New(constructor, argExprs),
				// Force expression to be type object so that DLR CallSite
				// code things only type object flows out of the CallSite.
					typeof(object)),
				target.Restrictions.Merge(BindingRestrictions.Combine(args))
								   .Merge(moreTests));
		}

	}

	class DoHelpersGetMemberBinder : GetMemberBinder
	{

		internal DoHelpersGetMemberBinder(string name) : base(name, true) { }

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			return errorSuggestion ??
				   new DynamicMetaObject(
						   Expression.Constant(DynamicObjectHelpers.Sentinel),
						   target.Restrictions.Merge(
							   BindingRestrictions.GetTypeRestriction(
								   target.Expression, target.LimitType)));
		}
	}

	class DoHelpersSetMemberBinder : SetMemberBinder
	{
		internal DoHelpersSetMemberBinder(string name) : base(name, true) { }

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			return errorSuggestion ??
				   DynamicObjectHelpers.CreateThrow(
					   target, null, BindingRestrictions.Empty,
					   typeof(MissingMemberException),
							  "If IDynObj doesn't support setting members, " +
							  "DOHelpers can't do it for the IDO.");
		}
	}
}
