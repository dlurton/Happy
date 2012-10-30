using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;
using HappyTemplate.Runtime;
using HappyTemplate.Runtime.Trackers;
using Microsoft.Scripting;
using Module = HappyTemplate.Compiler.Ast.Module;
using UnaryExpression = System.Linq.Expressions.UnaryExpression;

namespace HappyTemplate.Compiler.AstAnalyzer
{
	class AstAnalyzer_New
	{
		const string GlobalObjectName = "__global__";
		const string RuntimeContextIdentifier = "__runtimeContext__";

		Stack<HappySymbolTable> _scopeStack;
		ParameterExpression _globalScopeExp;
		readonly HappyLanguageContext _languageContext;
		AnalysisContext _analysisContext;

		readonly static string[] _defaultReferences = new[] {
		                                           		"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
		                                           		"System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
		                                           	};

		class LoopContext
		{
			public LabelTarget BreakLabel;
			public LabelTarget ContinueLabel;
		}

		Stack<LoopContext> _loopContextStack;

		void Init()
		{
			_scopeStack = new Stack<HappySymbolTable>();
			_globalScopeExp = Expression.Parameter(typeof(IDynamicMetaObjectProvider), GlobalObjectName);
			_loopContextStack = new Stack<LoopContext>();
		}

		

		public AstAnalyzer_New(HappyLanguageContext languageContext)
		{
			_languageContext = languageContext;
		}

		public HappyScriptCode Analyze(Module module, SourceUnit sourceUnit)
		{
			Init();

			//This List<Expression> becomes the global scope initializer

			var rootNamespaces = LoadAllAssemblies(module.LoadDirectives);

			ExpandoObject importScope = new ExpandoObject();
			foreach (HappyNamespaceTracker tracker in rootNamespaces.Values)
				DynamicObjectHelpers.SetMember(importScope, tracker.Name, tracker);

#if WRITE_AST
			AstWriter writer = new AstWriter(Console.Out);
			module.WriteString(writer);
#endif

			var getRuntimeContextExpr = Expression.Dynamic(_languageContext.CreateGetMemberBinder(RuntimeContextIdentifier, false), typeof(object), _globalScopeExp);
			UnaryExpression runtimeContextExpression = Expression.Convert(getRuntimeContextExpr, typeof(HappyRuntimeContext));
			var errorCollector = new ErrorCollector(_languageContext.ErrorSink);
			_analysisContext = new AnalysisContext(errorCollector, _languageContext, runtimeContextExpression, _globalScopeExp);

			RunAllVisitors(module, rootNamespaces);

			List<Expression> body = new List<Expression>();

			//Initialize globals
			using (_scopeStack.TrackPush(module.SymbolTable))
			{
				foreach (VariableDef def in module.GlobalDefStatements.SelectMany(defStmt => defStmt.VariableDefs))
				{
					Expression initialValue = def.InitializerExpression != null ? ExpressionAnalyzer.Analyze(_analysisContext, def.InitializerExpression) : Expression.Constant(null, typeof(object));
					HappySymbolBase symbol = module.SymbolTable.Items[def.Name.Text];
					body.Add(symbol.GetSetExpression(initialValue));
				}
			}

			body.AddRange(module.Functions.Select(
				func => _analysisContext.PropertyOrFieldSet(func.Name.Text, _globalScopeExp, FunctionAnalyzer.Analzye(_analysisContext, func))));

			//At this point analysis has completed and all of our stacks should be empty
			DebugAssert.AreEqual(0, _scopeStack.Count, "scope stack not empty after analysis");

			//Add an empty expression--prevents an exception by Expression.Lambda when body is empty.
			//This allows compilation of empty template sets.
			if(body.Count == 0)
				body.Add(Expression.Empty());

			LambdaExpression globalScopeInitializer = Expression.Lambda(typeof(Action<IDynamicMetaObjectProvider>),
																		Expression.Block(body), new[] { _globalScopeExp });

			HappyScriptCode output = new HappyScriptCode(sourceUnit, globalScopeInitializer.Compile());

			return output;
		}

		#region Privates

		void RunAllVisitors(Module module, Dictionary<string, HappyNamespaceTracker> rootNamespaces)
		{
			Visitors.AstVisitorBase[] visitors =
				{
					new Visitors.BuildSymbolTablesVisitor(_analysisContext, rootNamespaces),
					new Visitors.SemanticVisitor(_analysisContext.ErrorCollector),
					new Visitors.PreAnalyzeVisitor(_analysisContext.ErrorCollector)
				};

			foreach (var v in visitors)
				module.Accept(v);
		}


		/// <summary>
		/// This method loads the default assemblies and the assemblies specified in loadStatements.
		/// </summary>
		/// <param name="loadDirectives"></param>
		/// <returns>
		///	A dictionary containing the HappyNamespaceTrackers corresponding to the root namespaces in the loaded assemblies.
		/// </returns>
		Dictionary<string, HappyNamespaceTracker> LoadAllAssemblies(LoadDirective[] loadDirectives)
		{
			Dictionary<string, HappyNamespaceTracker> rootNamespaces = new Dictionary<string, HappyNamespaceTracker>();
			var assembliesToLoad = _defaultReferences.Union(loadDirectives.Select(ls => ls.AssemblyName));
			foreach (string name in assembliesToLoad)
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
					currentNamespaceTracker.SetMember(type.Name, new HappyTypeTracker( /*current,*/ type));
				}
			}
			return rootNamespaces;
		}
		
		#endregion
	}
}
