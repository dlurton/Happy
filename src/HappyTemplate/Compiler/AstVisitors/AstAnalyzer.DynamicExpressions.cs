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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HappyTemplate.Runtime;

namespace HappyTemplate.Compiler.AstVisitors
{
    partial class AstAnalyzer
    {
	    readonly Dictionary<string, Type> _callSiteDelegates = new Dictionary<string, Type>();
		int _callSiteCount;
	    
		Expression dynamicInvoke(Expression delegateExpr, Expression[] args)
		{
			var targetArgExprs = args.ToList();
			targetArgExprs.Insert(0, delegateExpr);

			return createCallSiteInvokeTarget(
				"CreateInvokeBinder", 
				new [] { getNewCallInfoExpr(args.Length) },
				targetArgExprs.ToArray());
		}

	    Expression dynamicBinaryExpression(ExpressionType expType, Expression lvalue, Expression rvalue)
		{
			//__languageContext.CreateBinaryOperationBinder(expType), typeof(object), lvalue, rvalue));
			return createCallSiteInvokeTarget(
				"CreateBinaryOperationBinder",
				new[] { Expression.Constant(expType, typeof(ExpressionType)) },
				new[] { lvalue, rvalue });
		}
	
		Expression dynamicCreate(Expression[] args)
		{
			//_languageContext.CreateCreateBinder(new CallInfo(node.ConstructorAgs.Length))
			return createCallSiteInvokeTarget(
				"CreateCreateBinder",
				new[] { getNewCallInfoExpr(args.Length) },
				args);
		}

		Expression dynamicGetMember(Expression instance, string name)
		{
			//_languageContext.CreateGetMemberBinder(name, false), typeof(object), new[] { instance }
			return createCallSiteInvokeTarget(
				"CreateGetMemberBinder",
				new[] { Expression.Constant(name, typeof(string)), Expression.Constant(false) },
				new[] { instance });
		}

		Expression dynamicSetMember(Expression instance, string name, Expression newValue)
		{
			//_languageContext.CreateSetMemberBinder(name, false)
			return createCallSiteInvokeTarget(
				"CreateSetMemberBinder",
				new[] { Expression.Constant(name, typeof(string)), Expression.Constant(false) },
				new[] { instance, newValue });
		}

		Expression dynamicSetIndex(Expression[] args)
		{
			//_languageContext.CreateSetIndexBinder(new CallInfo(args.Count))
			return createCallSiteInvokeTarget(
				"CreateSetIndexBinder",
				new[] { getNewCallInfoExpr(args.Length) },
				args);
		}

		Expression dynamicGetIndex(Expression[] args)
		{
			//_languageContext.CreateGetIndexBinder(new CallInfo(args.Count))
			return createCallSiteInvokeTarget(
				"CreateGetIndexBinder",
				new[] { getNewCallInfoExpr(args.Length) },
				args);
		}

		Expression dynamicCall(string methodName, Expression[] args)
		{
			//public override InvokeMemberBinder CreateCallBinder(string name, bool ignoreCase, CallInfo callInfo)
			return createCallSiteInvokeTarget(
				"CreateCallBinder",
				new [] { 
					Expression.Constant(methodName, typeof(string)), 
					Expression.Constant(false), 
					getNewCallInfoExpr(args.Length) 
				},
				args);
		}

		/// <summary>
		/// Creates a call site.
		/// </summary>
		/// <param name="createBinderMethodName">
		/// The name of the method 
		/// </param>
		/// <param name="instance"></param>
		/// <param name="targetArgExprs"></param>
		/// <remarks>
		///		<ul>
		///			<li>
		///				Determines if a delegate has been created in _assemblyGenerator yet that matches the 
		///				operation and signature of the call site to be created.  
		///			</li>
		///			<li>
		///				Uses existing delegate or defines a new delegate if one was not identified.
		///			</li>
		///			<li>
		///				Creates a field in the dynamic type for storing the call site
		///			</li>
		///			<li>
		///				Emits code to check if the field has been initialized with a binder yet, and if not initializes it 
		///				using the HappyLanguageContext method identified by createBinderMethodName, passing the arguments
		///				identified by createBinderMethodArgExprs.
		///			</li>
		///			<li>
		///				Emits code to invoke the callsite's target.  
		///			</li>
		///		</ul>
		/// </remarks>
		/// <returns></returns>
	    Expression createCallSiteInvokeTarget(string createBinderMethodName, Expression[] createBinderMethodArgExprs, Expression[] targetArgExprs)
		{
			var argTypes = new List<Type> { typeof(CallSite) };
			argTypes.AddRange(targetArgExprs.Select(a => a.Type));
			Type delegateType = getDelegateType(createBinderMethodName, typeof(object), argTypes.ToArray());

			Expression callSiteExpr = makeCallSiteField(delegateType);
			MethodInfo createBinderMethodInfo = typeof(HappyLanguageContext).GetMethod(createBinderMethodName, BindingFlags.Public | BindingFlags.Instance);
			DebugAssert.IsNotNull(createBinderMethodInfo, "'{0}' is not a valid member of HappyLanguageContext");
			var expectedArgumentTypes = createBinderMethodInfo.GetParameters().Select(a => a.ParameterType).ToArray();
#if DEBUG
			DebugAssert.AreEqual(expectedArgumentTypes.Length, createBinderMethodArgExprs.Length,
			                     "Incorrect number of expressions supplied as create binder method arugments for HappyLanguageContext.{0} " +
			                     "Expected {1} got {2}", createBinderMethodName, expectedArgumentTypes.Length, createBinderMethodArgExprs.Length);

			for(int i = 0; i < expectedArgumentTypes.Length; ++i)
			{
				if (!createBinderMethodArgExprs[i].Type.IsAssignableFrom(expectedArgumentTypes[i]))
					DebugAssert.Fail("Argument {0} ( of type {1}) in call to HappyLanguageContext.{2} was not asssignable from {3}",
									 i + 1, createBinderMethodArgExprs[i].Type.FullName, createBinderMethodName, expectedArgumentTypes[i].FullName);
			}
#endif
		    Expression initCallSiteExpr = getInitCallSiteExpr(callSiteExpr, createBinderMethodInfo, createBinderMethodArgExprs);

			List<Expression> finalArgs = new List<Expression> { callSiteExpr};
		    finalArgs.AddRange(targetArgExprs);

		    Expression callTargetExpr = Expression.Invoke(Expression.Field(callSiteExpr, "Target"), finalArgs);

			return Expression.Block(initCallSiteExpr, callTargetExpr);
	    }

	    Expression getInitCallSiteExpr(Expression callSiteExpr, MethodInfo createBinderMethodInfo, Expression[] createBinderMethodArgExprs)
	    {
		    var createBinderExpr = getCreateBinderExpr(createBinderMethodInfo, createBinderMethodArgExprs);
		    var createCallSiteExpr = Expression.Assign(callSiteExpr, getCreateCallSiteExpr(callSiteExpr.Type, createBinderExpr));
		    var initCallSiteExpr = Expression.IfThen(IsNullExpression(callSiteExpr), createCallSiteExpr);

		    return Expression.Block(initCallSiteExpr, callSiteExpr);
	    }

		MethodCallExpression getCreateBinderExpr(MethodInfo createBinderMethodInfo, Expression[] createBinderMethodArgExprs)
		{
			//var args = new List<Expression>();
			//args.AddRange(createBinderMethodArgExprs);
			//args.Add(typeOfObjectExpr());

			return Expression.Call(_languageContextExpr, createBinderMethodInfo, createBinderMethodArgExprs /*args*/);
		}

		MethodCallExpression getCreateCallSiteExpr(Type callSiteType, Expression binderExpr)
		{
			return Expression.Call(callSiteType.GetMethod("Create"), binderExpr);
		}

	    static Expression getNewCallInfoExpr(int numCallSiteArguments)
	    {
		    var callInfoConstructorInfo = typeof(CallInfo).GetConstructor(new[] { typeof(int), typeof(string[]) });
		    var newCallInfoExpr = 
				Expression.New(callInfoConstructorInfo, 
					Expression.Constant(numCallSiteArguments), 
					Expression.New(typeof(string[]).GetConstructor(new[] { typeof(int) }), Expression.Constant(0)));
		    return newCallInfoExpr;
	    }

	    Expression makeCallSiteField(Type delegateType)
	    {
		    Type callSiteType = typeof(CallSite<>).MakeGenericType(new[] { delegateType });
		    FieldInfo callSiteFieldInfo = _assemblyGenerator.DefineField("CallSite" + ++_callSiteCount, callSiteType);
		    return Expression.Field(null, callSiteFieldInfo);
	    }
		string getDelegateSignatureKey(string methodName, Type returnType, Type[] argTypes)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(methodName);
			builder.Append('|');
			builder.Append(returnType.AssemblyQualifiedName);
			builder.Append('|');
			foreach (var argType in argTypes)
			{
				builder.Append(argType.AssemblyQualifiedName);
				builder.Append('|');
			}

			return builder.ToString();
		}

		Type getDelegateType(string methodName, Type returnType, Type[] argTypes)
		{
			string delegateKey = getDelegateSignatureKey(methodName, returnType, argTypes);
			Type delegateType;
			if (!_callSiteDelegates.TryGetValue(delegateKey, out delegateType))
			{
				delegateType = _assemblyGenerator.DefineDelegate("CallSiteDelegate_" + _callSiteDelegates.Count + 1, returnType, argTypes);
				_callSiteDelegates.Add(delegateKey, delegateType);
			}
			return delegateType;
		}
    }
}

