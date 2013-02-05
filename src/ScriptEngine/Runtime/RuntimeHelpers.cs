/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using Happy.ScriptEngine.Runtime.Trackers;

namespace Happy.ScriptEngine.Runtime
{
	public static class RuntimeHelpers
	{
		/// <summary>
		/// TODO:  this does not load types that don't have namespace
		/// (a rare occurance but perhaps one we should account for?)
		/// </summary>
		/// <param name="name"></param>
		/// <param name="rootNamespaces"></param>
		public static void LoadAssembly(string name, Dictionary<string, HappyNamespaceTracker> rootNamespaces)
		{
			AssemblyName assemblyName = new AssemblyName(name);
			Assembly assembly = Assembly.Load(assemblyName);

			foreach (Type type in assembly.GetTypes().Where(t => t.Namespace != null))
			{
				string[] namespaceSegments = type.Namespace.Split('.');

				HappyNamespaceTracker currentNamespaceTracker;
				if (!rootNamespaces.TryGetValue(namespaceSegments[0], out currentNamespaceTracker))
				{
					currentNamespaceTracker = new HappyNamespaceTracker(null, namespaceSegments[0]);
					rootNamespaces.Add(namespaceSegments[0], currentNamespaceTracker);
				}

				foreach (string segment in namespaceSegments.Skip(1))
				{
					if (currentNamespaceTracker.HasMember(segment))
						currentNamespaceTracker = (HappyNamespaceTracker)currentNamespaceTracker.GetMember(segment);
					else
					{
						HappyNamespaceTracker next = new HappyNamespaceTracker(currentNamespaceTracker, segment);
						currentNamespaceTracker.SetMember(segment, next);
						currentNamespaceTracker = next;
					}
				}
				currentNamespaceTracker.SetMember(type.Name, new HappyTypeTracker(type));
			}
		}

		public static void PopulateRootNamespaces(dynamic globals, Dictionary<string, HappyNamespaceTracker> roots)
		{
			foreach(var kvp in roots)
			{
				DynamicObjectHelpers.SetMember(globals, kvp.Key, kvp.Value);
			}
		}

		public static void UseNamespace(dynamic globals, string @namespace)
		{
			var segments = @namespace.Split('.');
			object maybeNamespace = DynamicObjectHelpers.GetMember(globals, segments[0]);
			const string doesNotExistMessage = "'{0}' does not exist in any loaded assembly";
			const string isNotANamespaceMessage = "'{0}' is not a namespace";
			if(maybeNamespace == null)
			{
				throw new InvalidOperationException(String.Format(doesNotExistMessage, @namespace));
			}

			var current = maybeNamespace as HappyNamespaceTracker;
			
			if (current == null) //TODO:  better exception type
				throw new InvalidOperationException(String.Format(isNotANamespaceMessage, segments[0]));

			foreach(var seg in segments.Skip(1))
			{
				IHappyTracker maybeNamespace2;
				if(!current.TryGetMember(seg, out maybeNamespace2))
					throw new InvalidOperationException(String.Format(doesNotExistMessage, @namespace));
				current = maybeNamespace2 as HappyNamespaceTracker;
				if(current == null)
					throw new InvalidOperationException(String.Format(isNotANamespaceMessage, @namespace));
			}

			current.ForEachMember(tracker => DynamicObjectHelpers.SetMember(globals, tracker.Name, tracker));
		}

		public static bool HappyEq(object x, object y)
		{
			if (x == null)
				return y == null;
			if (y == null)
				return false;
			
			var xtype = x.GetType();
			var ytype = y.GetType();
			if ((xtype.IsPrimitive || xtype == typeof(string) && (ytype.IsPrimitive || ytype == typeof(string))))
				return x.Equals(y);
		
			if(xtype.IsEnum || ytype.IsEnum)
				return x.Equals(y);
		
			return ReferenceEquals(x, y);
		}

		public static IEnumerable GetWhereEnumerable(IEnumerable enumerable, Func<object, bool> test)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (test(enumerator.Current))
					yield return enumerator.Current;
			}
		}

		///////////////////////////////////////
		// Utilities used by binders at runtime
		///////////////////////////////////////

		// ParamsMatchArgs returns whether the args are assignable to the parameters.
		// We specially check for our TypeModel that wraps .NET's RuntimeType, and
		// elsewhere we detect the same situation to convert the TypeModel for calls.
		//
		// Consider checking p.IsByRef and returning false since that's not CLS.
		//
		// Could check for a.HasValue and a.Value is None and
		// ((paramtype is class or interface) or (paramtype is generic and
		// nullable<t>)) to support passing nil anywhere.
		//
		public static bool ParametersMatchArguments(ParameterInfo[] parameters,
			DynamicMetaObject[] args)
		{
			// We only call this after filtering members by this constraint.
			Debug.Assert(args.Length == parameters.Length,
				"Internal: args are not same len as params?!");
			for (int i = 0; i < args.Length; i++)
			{
				var paramType = parameters[i].ParameterType;
				// We consider arg of TypeModel and param of Type to be compatible.
				//if (paramType == typeof(Type) && (args[i].LimitType == typeof(TypeModel)))
				//{
				//    continue;
				//}
				if (!paramType
					// Could check for HasValue and Value==null AND
					// (paramtype is class or interface) or (is generic
					// and nullable<T>) ... to bind nullables and null.
					.IsAssignableFrom(args[i].LimitType))
				{
					return false;
				}
			}
			return true;
		}

		// Returns a DynamicMetaObject with an expression that fishes the .NET
		// RuntimeType object from the TypeModel MO.
		//
		//public static DynamicMetaObject GetRuntimeTypeMoFromModel(
		//    DynamicMetaObject typeModelMO)
		//{
		//    Debug.Assert((typeModelMO.LimitType == typeof(TypeModel)),
		//        "Internal: MO is not a TypeModel?!");
		//    // Get tm.ReflType
		//    var pi = typeof(TypeModel).GetProperty("ReflType");
		//    Debug.Assert(pi != null);
		//    return new DynamicMetaObject(
		//        Expression.Property(
		//            Expression.Convert(typeModelMO.Expression, typeof(TypeModel)),
		//            pi),
		//        typeModelMO.Restrictions.Merge(
		//            BindingRestrictions.GetTypeRestriction(
		//                typeModelMO.Expression, typeof(TypeModel)))//,
		//        // Must supply a value to prevent binder FallbackXXX methods
		//        // from infinitely looping if they do not check this MO for
		//        // HasValue == false and call Defer.  After BinderFactory added Defer
		//        // checks, we could verify, say, FallbackInvokeMember by no
		//        // longer passing a value here.
		//        //((TypeModel)typeModelMO.Value).ReflType
		//        );
		//}

		// Returns list of Convert exprs converting args to param types.  If an arg
		// is a TypeModel, then we treat it special to perform the binding.  We need
		// to map from our runtime model to .NET's RuntimeType object to match.
		//
		// To call this function, args and pinfos must be the same length, and param
		// types must be assignable from args.
		//
		// NOTE, if using this function, then need to use GetTargetArgsRestrictions
		// and make sure you're performing the same conversions as restrictions.
		//
		public static Expression[] ConvertArguments(
			DynamicMetaObject[] args, ParameterInfo[] ps)
		{
			Debug.Assert(args.Length == ps.Length, "Internal: args are not same len as params?!");
			Expression[] callArgs = new Expression[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				callArgs[i] = Expression.Convert(args[i].Expression, ps[i].ParameterType);
			}
			return callArgs;
		}

		// GetTargetArgsRestrictions generates the restrictions needed for the
		// MO resulting from binding an operation.  This combines all existing
		// restrictions and adds some for arg conversions.  targetInst indicates
		// whether to restrict the target to an instance (for operations on type
		// objects) or to a type (for operations on an instance of that type).
		//
		// NOTE, this function should only be used when the caller is converting
		// arguments to the same types as these restrictions.
		//
		public static BindingRestrictions GetTargetArgsRestrictions(
			DynamicMetaObject target, DynamicMetaObject[] args,
			bool instanceRestrictionOnTarget)
		{
			// Important to add existing restriction first because the
			// DynamicMetaObjects (and possibly values) we're looking at depend
			// on the pre-existing restrictions holding true.
			var restrictions = target.Restrictions.Merge(BindingRestrictions
				.Combine(args));
			if (instanceRestrictionOnTarget)
			{
				restrictions = restrictions.Merge(
					BindingRestrictions.GetInstanceRestriction(
						target.Expression,
						target.Value
						));
			}
			else
			{
				restrictions = restrictions.Merge(
					BindingRestrictions.GetTypeRestriction(
						target.Expression,
						target.LimitType
						));
			}
			for (int i = 0; i < args.Length; i++)
			{
				BindingRestrictions r;
				if (args[i].HasValue && args[i].Value == null)
				{
					r = BindingRestrictions.GetInstanceRestriction(
						args[i].Expression, null);
				}
				else
				{
					r = BindingRestrictions.GetTypeRestriction(
						args[i].Expression, args[i].LimitType);
				}
				restrictions = restrictions.Merge(r);
			}
			return restrictions;
		}

		// Return the expression for getting target[indexes]
		//
		// Note, callers must ensure consistent restrictions are added for
		// the conversions on args and target.
		//
		public static Expression GetIndexingExpression(
			DynamicMetaObject target,
			DynamicMetaObject[] indexes)
		{
			Debug.Assert(target.HasValue && target.LimitType != typeof(Array));

			var indexExpressions = indexes.Select(
				i => Expression.Convert(i.Expression, i.LimitType))
				.ToArray();

			//// CONS
			//if (target.LimitType == typeof(Cons))
			//{
			//    // Call RuntimeHelper.GetConsElt
			//    var args = new List<Expression>();
			//    // The first argument is the list
			//    args.Add(
			//        Expression.Convert(
			//            target.Expression,
			//            target.LimitType)
			//    );
			//    args.AddRange(indexExpressions);
			//    return Expression.Call(
			//        typeof(RuntimeHelpers),
			//        "GetConsElt",
			//        null,
			//        args.ToArray());
			//    // ARRAY
			//}
			//else 
			if (target.LimitType.IsArray)
			{
				return Expression.ArrayAccess(
					Expression.Convert(target.Expression,
						target.LimitType),
					indexExpressions
					);
				// INDEXER
			}
			else
			{
				var props = target.LimitType.GetProperties();
				var indexers = props.Where(p => p.GetIndexParameters().Length > 0).ToArray();
				indexers = indexers.Where(idx => idx.GetIndexParameters().Length == indexes.Length).ToArray();

				var res = new List<PropertyInfo>();
				foreach (var idxer in indexers)
				{
					if (RuntimeHelpers.ParametersMatchArguments(
						idxer.GetIndexParameters(), indexes))
					{
						// all parameter types match
						res.Add(idxer);
					}
				}
				if (res.Count == 0)
				{
					return Expression.Throw(
						Expression.New(
							typeof(MissingMemberException)
								.GetConstructor(new Type[] { typeof(string) }),
							Expression.Constant(
								"Can't bind because there is no matching indexer.")
							)
						);
				}
				return Expression.MakeIndex(
					Expression.Convert(target.Expression, target.LimitType),
					res[0], indexExpressions);
			}
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
		public static DynamicMetaObject CreateThrow(DynamicMetaObject target, DynamicMetaObject[] args, BindingRestrictions moreTests,Type exception, params object[] exceptionArgs)
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

		///<summary>
		/// EnsureObjectResult wraps expr if necessary so that any binder or 
		/// DynamicMetaObject result expression returns object.  This is required by CallSites.
		///</summary>

		public static Expression EnsureObjectResult(Expression expr)
		{
			if (!expr.Type.IsValueType)
				return expr;
			if (expr.Type == typeof(void))
				return Expression.Block(
					expr, Expression.Default(typeof(object)));
			

			return Expression.Convert(expr, typeof(object));
		}

		public static Expression EnsureBoolResult(Expression expression)
		{
			if(expression.Type != typeof(bool))
				return Expression.Convert(expression, typeof(bool));

			return expression;
		}
	}
}

// namespace

