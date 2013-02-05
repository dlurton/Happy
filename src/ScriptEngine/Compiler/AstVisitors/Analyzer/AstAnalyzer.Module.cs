/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Happy.ScriptEngine.Compiler.Ast;
using Happy.ScriptEngine.Compiler.AstVisitors.BinaryExpressions;
using Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables;
using Happy.ScriptEngine.Runtime;
using Happy.ScriptEngine.Runtime.Trackers;
using Microsoft.Scripting;

namespace Happy.ScriptEngine.Compiler.AstVisitors.Analyzer
{
	partial class AstAnalyzer
	{
		const string GeneratedAssemblyName = "TemporaryDynamicAssembly";
		const string GeneratedLanguageContextFieldName = "LanguageContext";
		const string GeneratedTypeName = "HappyClass";
		const string GeneratedModuleName = "HappyDynamicAssembly";
		const string GeneratedRuntimeContextInitializerMethodName = "GetRuntimeContextInitializer";

		static readonly string[] _defaultAssembliesToLoad = new[]
		{
			"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
			"System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
			"Happy.RuntimeLib"
		};

		Expression _languageContextExpr;
		readonly ParameterExpression _rootNamespaceDict = Expression.Parameter(typeof(Dictionary<string, HappyNamespaceTracker>), "rootNamespaces");
		readonly AssemblyGenerator _assemblyGenerator = new AssemblyGenerator(GeneratedAssemblyName);
		string[] _assembliesToLoad;


		/// <summary>
		/// This method loads the default assemblies and the assemblies specified in loadStatements.
		/// </summary>
		/// <param name="loadDirectives"></param>
		/// <returns>
		///	A dictionary containing the HappyNamespaceTrackers corresponding to the root namespaces in the loaded assemblies.
		/// </returns>
		Dictionary<string, HappyNamespaceTracker> LoadAllAssemblies(IEnumerable<LoadDirective> loadDirectives)
		{
			_assembliesToLoad = _defaultAssembliesToLoad.Union(loadDirectives.Select(ls => ls.AssemblyName)).ToArray();
			Dictionary<string, HappyNamespaceTracker> rootNamespaces = new Dictionary<string, HappyNamespaceTracker>();
			
			foreach (string name in _assembliesToLoad)
			{
				//TODO: May be we can come up with a simplified version of this method just to determine
				//the root namespaces?
				RuntimeHelpers.LoadAssembly(name, rootNamespaces);
			}
			return rootNamespaces;
		}

		public HappyLambdaScriptCode Analyze(Module module, SourceUnit sourceUnit)
		{
			Dictionary<string, HappyNamespaceTracker> rootNamespaces = LoadAllAssemblies(module.LoadDirectives);

			AstVisitorBase[] visitors =
				{
					new BinaryExpressionFixerVisitor(),
					new BuildSymbolTablesVisitor(this, _errorCollector, rootNamespaces),
					new ResolveSymbolsVisitor(_errorCollector),
					new SemanticVisitor(_errorCollector)
				};

			foreach (var v in visitors)
				module.Accept(v);

			prepareAssemblyGenerator();

			module.Accept(this);

			Expression expression = _expressionStack.Pop();
			DebugAssert.IsZero(_expressionStack.Count, "AstAnalyzer didn't consume all expressions on the stack");

			var runtimeContextInitializer = (LambdaExpression)expression;
			return new HappyLambdaScriptCode(sourceUnit, compileDynamicAssembly(runtimeContextInitializer));
		}

		void prepareAssemblyGenerator()
		{
			_assemblyGenerator.DefineModule(GeneratedModuleName, true);
			_assemblyGenerator.DefineType(GeneratedTypeName);
			var languageContextField = _assemblyGenerator.DefineField(GeneratedLanguageContextFieldName, typeof(HappyLanguageContext));
			_languageContextExpr = Expression.Field(null, languageContextField);
		}

		Action<HappyRuntimeContext> compileDynamicAssembly(LambdaExpression runtimeContextInitializer)
		{
			var rciExpr = Expression.Lambda(runtimeContextInitializer);
			_assemblyGenerator.DefineMethod(GeneratedRuntimeContextInitializerMethodName, rciExpr);
			Type t = _assemblyGenerator.CompleteType();

			var mi = t.GetMethod(GeneratedRuntimeContextInitializerMethodName);
			var actionGetter = (Func<Action<HappyRuntimeContext>>)mi.Invoke(null, null);

			t.GetField(GeneratedLanguageContextFieldName).SetValue(null, _languageContext);

			return actionGetter();
		}

		public override void AfterVisit(Module node)
		{
			//body contains a array of expressions, that when compiled, populate a single expando object
			//with the functions and default values of the global variables.
			//and more
			List<Expression> body = new List<Expression>();

			var functions = _expressionStack.Pop(node.Functions.Length);
			var globalDefStatements = _expressionStack.Pop(node.GlobalDefStatements.Length);
			var useStatements = _expressionStack.Pop(node.UseStatements.UseStatements.Length, false);
			var loadDirectives = _expressionStack.Pop(node.LoadDirectives.Length);

			emitLoadAssemblies(body, loadDirectives);
			emitUseStatements(body, useStatements);
			body.AddRange(globalDefStatements);
			body.AddRange(functions);

			//Add an empty expression--prevents an exception by Expression.Lambda when body is empty.
			//This allows compilation of empty template sets.
			body.Add(Expression.Empty());

			_expressionStack.Push(node, 
				Expression.Lambda(
					typeof(Action<HappyRuntimeContext>), 
					Expression.Block(new[] {_rootNamespaceDict}, body), 
					new[] { _runtimeContextExp }));
		}

		void emitUseStatements(List<Expression> body, IEnumerable<Expression> useStatements)
		{
			foreach (var @namespace in BuildSymbolTablesVisitor.DefaultUseNamespaces)
				body.Add(getUseStatementCall(@namespace));

			body.AddRange(useStatements);
		}

		void emitLoadAssemblies(List<Expression> body, IEnumerable<Expression> loadDirectives)
		{
			body.Add(Expression.Assign(_rootNamespaceDict, Expression.New(typeof(Dictionary<string, HappyNamespaceTracker>))));

			foreach (var defaultAss in _defaultAssembliesToLoad)
				body.Add(getLoadAssemblyCall(defaultAss));

			body.AddRange(loadDirectives);

			body.Add(Expression.Call(
					typeof(RuntimeHelpers),
					"PopulateRootNamespaces",
					null,
					_globalScopeExp, 
					_rootNamespaceDict)); 
		}

		public override void Visit(LoadDirective node)
		{
			var assemblyName = node.AssemblyName;
			_expressionStack.Push(node, getLoadAssemblyCall(assemblyName));
			base.Visit(node);
		}

		MethodCallExpression getLoadAssemblyCall(string assemblyName)
		{
			return Expression.Call(
				typeof(RuntimeHelpers), 
				"LoadAssembly", 
				null, 
				Expression.Constant(assemblyName),
				_rootNamespaceDict);
		}

		public override void Visit(UseStatement node)
		{
			var callExpr = getUseStatementCall(String.Join(".", node.NamespaceSegments));
			_expressionStack.Push(node, callExpr);
		}

		MethodCallExpression getUseStatementCall(string @namespace)
		{
			return Expression.Call(
				typeof(RuntimeHelpers),
				"UseNamespace",
				null,
				_globalScopeExp, 
				Expression.Constant(@namespace));
		}
	}
}



