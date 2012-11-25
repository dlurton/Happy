/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

#if DEBUG
//#define WRITE_AST
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.Visitors;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;
using HappyTemplate.Runtime.Trackers;
using Microsoft.Scripting;
using System.Linq.Expressions;
using BinaryExpression = HappyTemplate.Compiler.Ast.BinaryExpression;
using Module = HappyTemplate.Compiler.Ast.Module;
using SwitchCase = HappyTemplate.Compiler.Ast.SwitchCase;
using UnaryExpression = HappyTemplate.Compiler.Ast.UnaryExpression;

namespace HappyTemplate.Compiler
{
	class AstAnalyzer_old
	{
		const string GlobalObjectName = "__global__";
		const string RuntimeContextIdentifier = "__runtimeContext__";
		public const string CurrentOutputIdentifier = "__currentOutput__";

		readonly static string[] DefaultReferences = new[] {
														"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
														"System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
													};

		ParameterExpression _globalScopeExp;
		Expression _runtimeContextExp;
		Stack<HappySymbolTable> _scopeStack;
		ParameterExpression _currentOutputExp;
		readonly HappyLanguageContext _languageContext;
		LabelTarget _returnLabelTarget;
		readonly PropertyInfo _outputWriterProperty;


		readonly ErrorCollector _errorCollector;

		class LoopContext
		{
			public LabelTarget BreakLabel;
			public LabelTarget ContinueLabel;
		}

		Stack<LoopContext> _loopContextStack;

		public AstAnalyzer_old(HappyLanguageContext languageContext)
		{
			_languageContext = languageContext;

			_errorCollector = new ErrorCollector(languageContext.ErrorSink);
			_outputWriterProperty = typeof(HappyRuntimeContext).GetProperty("OutputWriter");
		}


		void Init()
		{
			_scopeStack = new Stack<HappySymbolTable>();
			_globalScopeExp = Expression.Parameter(typeof(IDynamicMetaObjectProvider), GlobalObjectName);
			_runtimeContextExp = Expression.Convert(this.PropertyOrFieldGet(RuntimeContextIdentifier, _globalScopeExp), typeof(HappyRuntimeContext));
			_loopContextStack = new Stack<LoopContext>();
		}

		#region Analysis Dispatcher
		private Expression Analyze(AstNodeBase node)
		{
			Expression retval;
			if (node == null)
				return null;

			switch (node.NodeKind)
			{
				case AstNodeKind.BinaryExpression:
					retval = this.Analyze((BinaryExpression)node);
					break;
				case AstNodeKind.ForStatement:
					retval = this.Analyze((ForStatement)node);
					break;
				case AstNodeKind.IfStatement:
					retval = this.Analyze((IfStatement)node);
					break;
				case AstNodeKind.LiteralExpression:
					retval = this.Analyze((LiteralExpression)node);
					break;
				case AstNodeKind.NullExpression:
					retval = this.Analyze((NullExpression)node);
					break;
				case AstNodeKind.FunctionCallExpression:
					retval = this.Analyze((FunctionCallExpression)node);
					break;
				case AstNodeKind.IdentifierExpression:
					retval = this.Analyze((IdentifierExpression)node);
					break;
				case AstNodeKind.VerbatimSection:
					retval = this.Analyze((VerbatimSection)node);
					break;
				case AstNodeKind.OutputWrite:
					retval = this.Analyze((OutputStatement)node);
					break;
				case AstNodeKind.AnonymousTemplate:
					retval = this.Analyze((AnonymousTemplate)node);
					break;
				case AstNodeKind.ReturnStatement:
					retval = this.Analyze((ReturnStatement)node);
					break;
				case AstNodeKind.DefStatement:
					retval = this.Analyze((DefStatement)node);
					break;
				case AstNodeKind.NewObjectExpression:
					retval = this.Analyze((NewObjectExpression)node);
					break;
				case AstNodeKind.UnaryExpression:
					retval = this.Analyze((UnaryExpression)node);
					break;
				case AstNodeKind.BlockStatement:
					retval = this.Analyze((StatementBlock)node);
					break;
				case AstNodeKind.BreakStatement:
					retval = this.Analyze((BreakStatement)node);
					break;
				case AstNodeKind.ContinueStatement:
					retval = this.Analyze((ContinueStatement)node);
					break;
				case AstNodeKind.SwitchStatement:
					retval = this.Analyze((SwitchStatement)node);
					break;
				default:
					throw new UnhandledCaseException();
			}
			return retval;
		}

		#endregion

		public Expression Analyze(BreakStatement node)
		{
			DebugAssert.IsNonZero(_loopContextStack.Count, "break statement within loop");
			return Expression.Break(_loopContextStack.Peek().BreakLabel);
		}

		public Expression Analyze(ContinueStatement node)
		{
			DebugAssert.IsNonZero(_loopContextStack.Count, "break statement within loop");
			return Expression.Continue(_loopContextStack.Peek().ContinueLabel);
		}


		#region Analyzers

		public HappyScriptCode Analyze(Module module, SourceUnit sourceUnit)
		{
			Init();

			//This List<Expression> becomes the global scope initializer

			var rootNamespaces = LoadAllAssemblies(module.LoadDirectives);

			ExpandoObject importScope = new ExpandoObject();
			foreach (HappyNamespaceTracker tracker in rootNamespaces.Values)
				DynamicObjectHelpers.SetMember(importScope, tracker.Name, tracker);

			RunAllVisitors(module, rootNamespaces);


#if WRITE_AST
			AstWriter writer = new AstWriter(Console.Out);
			module.WriteString(writer);
#endif
			List<Expression> body = new List<Expression>();


			//Initialize globals
			using (_scopeStack.TrackPush(module.SymbolTable))
			{
				foreach (VariableDef def in module.GlobalDefStatements.SelectMany(defStmt => defStmt.VariableDefs))
				{
					Expression initialValue = def.InitializerExpression != null ? Analyze(def.InitializerExpression) : Expression.Constant(null, typeof(object));
					HappySymbolBase symbol = module.SymbolTable.Items[def.Name.Text];
					body.Add(symbol.GetSetExpression(initialValue));
				}
			}

			body.AddRange(module.Functions.Select(t => this.PropertyOrFieldSet(t.Name.Text, _globalScopeExp, this.Analyze(t))));

			//At this point analysis has completed and all of our stacks should be empty
			DebugAssert.AreEqual(0, _scopeStack.Count, "scope stack not empty after analysis");

			//Add an empty expression--prevents an exception by Expression.Lambda when body is empty.
			//This allows compilation of empty template sets.
			body.Add(Expression.Empty());

			LambdaExpression globalScopeInitializer = Expression.Lambda(typeof(Action<IDynamicMetaObjectProvider>),
																		Expression.Block(body), new[] { _globalScopeExp });


			HappyScriptCode output = new HappyScriptCode(sourceUnit, globalScopeInitializer.Compile());

			return output;
		}

		LambdaExpression Analyze(Function function)
		{
			_currentOutputExp = Expression.Parameter(typeof(TextWriter), CurrentOutputIdentifier);

			List<Expression> outerExpressions = new List<Expression>
			{
				Expression.Assign(_currentOutputExp, Expression.Property(_runtimeContextExp, _outputWriterProperty))
			};

			using (_scopeStack.TrackPush(function.ParameterList.SymbolTable))
			{
				//Create return target
				_returnLabelTarget = Expression.Label(typeof(object), "lambdaReturn");
				LabelExpression returnLabel = Expression.Label(_returnLabelTarget, Expression.Default(typeof(object)));

				outerExpressions.Add(this.Analyze(function.Body));
				outerExpressions.Add(returnLabel);

				BlockExpression completeFunctionBody = Expression.Block(new[] { _currentOutputExp }, outerExpressions);
				LambdaExpression lambda = Expression.Lambda(completeFunctionBody, function.Name.Text, function.ParameterList.SymbolTable.GetParameterExpressions());
				return lambda;
			}
		}

		BlockExpression Analyze(StatementBlock statementBlock)
		{
			return null;
			//using (_scopeStack.TrackPush(statementBlock.SymbolTable))
			//{
			//	List<Expression> expressions = new List<Expression>();
			//	expressions.AddRange(statementBlock.Statements.Select(this.Analyze));

			//	IEnumerable<Expression> expressionsToInclude = expressions.Where(exp => exp != null);

			//	//if (!statementBlock.GetHashCode())
			//		return Expression.Block(statementBlock.SymbolTable.GetParameterExpressions(), expressionsToInclude);

			//	return Expression.Block(expressionsToInclude);
			//}
		}

		Expression Analyze(AnonymousTemplate anonTemplate)
		{
			List<Expression> body = new List<Expression>();
			var previousCurrentOutputExp = _currentOutputExp;
			_currentOutputExp = Expression.Parameter(typeof(TextWriter), CurrentOutputIdentifier);

			using (_scopeStack.TrackPush(anonTemplate.Body.SymbolTable))
			{
				body.Add(PushWriter());
				body.Add(Expression.Assign(_currentOutputExp, Expression.Property(_runtimeContextExp, _outputWriterProperty)));

				body.Add(this.Analyze(anonTemplate.Body));
				body.Add(PopWriter());

				var tmpOutputExpression = _currentOutputExp;
				_currentOutputExp = previousCurrentOutputExp;


				return Expression.Block(this.TopSymbolTable.GetParameterExpressions().Union(new[] { tmpOutputExpression }), body);
			}
		}

		Expression Analyze(OutputStatement outputStatement)
		{
			List<Expression> writeExps = new List<Expression>();
			foreach (ExpressionNodeBase expression in outputStatement.ExpressionsToWrite)
				writeExps.Add(WriteToTextWriter(_currentOutputExp, this.Analyze(expression)));

			return Expression.Block(writeExps);
		}

		Expression Analyze(VerbatimSection node)
		{
			return WriteToTextWriter(_currentOutputExp, node.Text);
		}

		Expression Analyze(NullExpression node)
		{
			return Expression.Constant(null, typeof(object));
		}

		Expression Analyze(LiteralExpression node)
		{
			return Expression.Constant(node.Value, typeof(object));
		}

		Expression Analyze(IfStatement node)
		{
			Expression falseBlock = null;
			Expression condition = this.Analyze(node.Condition);

			Expression trueBlock = this.Analyze(node.TrueStatementBlock);

			if (node.FalseStatementBlock != null)
				falseBlock = this.Analyze(node.FalseStatementBlock);

			Expression conditionalExp = Expression.Convert(condition, typeof(Boolean));

			if (falseBlock == null)
				return Expression.IfThen(conditionalExp, trueBlock);

			return Expression.IfThenElse(conditionalExp, trueBlock, falseBlock);
		}

		Expression Analyze(SwitchStatement node)
		{
			if (node.Cases.Length == 0)
				return this.Analyze(node.SwitchExpression);

			List<System.Linq.Expressions.SwitchCase> cases = new List<System.Linq.Expressions.SwitchCase>();
			cases.AddRange(node.Cases.Select(this.AnalyzeSwitchCase));
			return Expression.Switch(this.Analyze(node.SwitchExpression),
				node.DefaultStatementBlock == null ? null : this.Analyze(node.DefaultStatementBlock),
				typeof(RuntimeHelpers).GetMethod("HappyEq"), cases);
		}

		System.Linq.Expressions.SwitchCase AnalyzeSwitchCase(SwitchCase @case)
		{
			Expression statements = @case.CaseStatementBlock.Statements.Length > 0 ? this.Analyze(@case.CaseStatementBlock) : (Expression)Expression.Empty();
			return Expression.SwitchCase(statements, @case.CaseValues.Select(this.Analyze));
		}

		Expression Analyze(IdentifierExpression node, Expression value = null)
		{
			var searchResult = this.TopSymbolTable.FindInScopeTree(node.Identifier.Text);
			if (searchResult == null)
			{
				_errorCollector.UndefinedVariable(node.Identifier);
				return Expression.Empty();
			}

			return value == null ? searchResult.GetGetExpression() : searchResult.GetSetExpression(value);
		}

		Expression Analyze(UnaryExpression node)
		{
			switch (node.Operator.Operation)
			{
				case OperationKind.Not:
					return Expression.MakeUnary(ToExpressionType(node.Operator), Expression.Convert(this.Analyze(node.Value), typeof(bool)), typeof(object));
				default:
					throw new UnhandledCaseException("Semantic checking should have prevented this unhandled case");
			}
		}

		Expression Analyze(BinaryExpression node)
		{
			if (node.Operator.Operation == OperationKind.MemberAccess)
			{
				Expression lvalue = this.Analyze(node.LeftValue);
				switch (node.RightValue.NodeKind)
				{
					case AstNodeKind.IdentifierExpression:
						string propName = ((IdentifierExpression)node.RightValue).Identifier.Text;
						return this.PropertyOrFieldGet(propName, lvalue);
					case AstNodeKind.FunctionCallExpression:
						FunctionCallExpression te = (FunctionCallExpression)node.RightValue;
						List<Expression> args = te.Arguments.Select(this.Analyze).ToList();

						args.Insert(0, lvalue);
						return Expression.Dynamic(
							_languageContext.CreateCallBinder(te.Identifier.Text, false, new CallInfo(args.Count)), typeof(object), args);
				}
			}


			ExpressionType expType = ToExpressionType(node.Operator);

			switch (expType)
			{
				case ExpressionType.Index:
					ArgumentList argList = (ArgumentList)node.RightValue;
					List<Expression> args = argList.Arguments.Select(this.Analyze).ToList();
					args.Insert(0, this.Analyze(node.LeftValue));
					return Expression.Dynamic(_languageContext.CreateGetIndexBinder(new CallInfo(args.Count)), typeof(object), args);

			}
			Expression rvalue = RuntimeHelpers.EnsureObjectResult(this.Analyze(node.RightValue));

			switch (expType)
			{
				case ExpressionType.Assign:
					if (node.LeftValue.NodeKind == AstNodeKind.BinaryExpression)
					{
						BinaryExpression leftBinary = (BinaryExpression)node.LeftValue;
						switch (leftBinary.Operator.Operation)
						{
							case OperationKind.MemberAccess: //Used to be MemberGet
								IdentifierExpression memberExpressionBase = leftBinary.RightValue as IdentifierExpression;
								if (memberExpressionBase == null)
								{
									_errorCollector.InvalidLValueForAssignment(leftBinary.Location);
									return Expression.Empty();
								}
								return this.PropertyOrFieldSet(memberExpressionBase.Identifier.Text, this.Analyze(leftBinary.LeftValue), rvalue);

							case OperationKind.Index:
								ArgumentList argList = (ArgumentList)leftBinary.RightValue;
								List<Expression> args = argList.Arguments.Select(this.Analyze).ToList();
								args.Insert(0, this.Analyze(leftBinary.LeftValue));
								args.Add(rvalue);
								return Expression.Dynamic(_languageContext.CreateSetIndexBinder(new CallInfo(args.Count)), typeof(object), args);
						}
						_errorCollector.InvalidLValueForAssignment(leftBinary.Location);
						return Expression.Empty();
					}

					if (node.LeftValue.NodeKind == AstNodeKind.IdentifierExpression)
					{
						return this.Analyze((IdentifierExpression)node.LeftValue, rvalue);
					}


					throw new UnhandledCaseException();
				case ExpressionType.OrElse:
					return Expression.OrElse(RuntimeHelpers.EnsureBoolResult(this.Analyze(node.LeftValue)), RuntimeHelpers.EnsureBoolResult(rvalue));
				case ExpressionType.AndAlso:
					return Expression.AndAlso(RuntimeHelpers.EnsureBoolResult(this.Analyze(node.LeftValue)), RuntimeHelpers.EnsureBoolResult(rvalue));
			}

			return Expression.Dynamic(_languageContext.CreateBinaryOperationBinder(expType), typeof(object), this.Analyze(node.LeftValue), rvalue);
		}

		static ExpressionType ToExpressionType(Operator node)
		{
			ExpressionType expType;
			//lvalue = this.Analyze(node.LeftValue);

			switch (node.Operation)
			{
				case OperationKind.Add:
					expType = ExpressionType.Add;
					break;
				case OperationKind.Subtract:
					expType = ExpressionType.Subtract;
					break;
				case OperationKind.Divide:
					expType = ExpressionType.Divide;
					break;
				case OperationKind.Multiply:
					expType = ExpressionType.Multiply;
					break;
				case OperationKind.Mod:
					expType = ExpressionType.Modulo;
					break;
				case OperationKind.LogicalAnd:
					expType = ExpressionType.AndAlso;
					break;
				case OperationKind.LogicalOr:
					expType = ExpressionType.OrElse;
					break;
				case OperationKind.Xor:
					expType = ExpressionType.ExclusiveOr;
					break;
				case OperationKind.Equal:
					expType = ExpressionType.Equal;
					break;
				case OperationKind.Greater:
					expType = ExpressionType.GreaterThan;
					break;
				case OperationKind.Less:
					expType = ExpressionType.LessThan;
					break;
				case OperationKind.GreaterThanOrEqual:
					expType = ExpressionType.GreaterThanOrEqual;
					break;
				case OperationKind.LessThanOrEqual:
					expType = ExpressionType.LessThanOrEqual;
					break;
				case OperationKind.NotEqual:
					expType = ExpressionType.NotEqual;
					break;
				case OperationKind.Assign:
					expType = ExpressionType.Assign;
					break;
				case OperationKind.Not:
					expType = ExpressionType.Not;
					break;
				case OperationKind.BitwiseAnd:
					expType = ExpressionType.And;
					break;
				case OperationKind.BitwiseOr:
					expType = ExpressionType.Or;
					break;
				case OperationKind.Index:
					expType = ExpressionType.Index;
					break;
				default:
					throw new UnhandledCaseException(node.Operation.ToString());
			}
			return expType;
		}

		Expression Analyze(ForStatement node)
		{
			ParameterExpression loopVariable = node.LoopVariableSymbol.GetGetExpression() as ParameterExpression;
			DebugAssert.IsNotNull(loopVariable, "loopVariableSymbol.GetGetExpression() did not return a ParameterExpression");
			ParameterExpression enumerator = Expression.Parameter(typeof(IEnumerator), "enumerator");
			Expression enumerable = this.Analyze(node.Enumerable);
			List<Expression> loop = new List<Expression>();
			Expression loopBody, between;
			
			LabelTarget breakTarget = Expression.Label("break");
			LabelTarget continueTarget = Expression.Label("continue");
			LoopContext context = new LoopContext { BreakLabel = breakTarget, ContinueLabel = continueTarget };
			_loopContextStack.Push(context);

			node.LoopBody.SetAnalyzeSymbolsExternally(true);

			using (_scopeStack.TrackPush(node.LoopBody.SymbolTable))
			{
				between = this.Analyze(node.Between);

				Expression getEnumerator = AnalyzeGetEnumerator(node, enumerable);

				loop.Add(Expression.Assign(enumerator, getEnumerator));
				loopBody = this.Analyze(node.LoopBody);
			}

			//The loop "wrapper", which wraps the actual loop body
			//It's purpose is to assign the loopVariable after each iteration and 
			//to write the between clause to the current output 
			BlockExpression loopWrapper = Expression.Block(
				Expression.Assign(loopVariable, Expression.Property(enumerator, typeof(IEnumerator).GetProperty("Current"))),
				loopBody,
				Expression.Label(continueTarget),
				Expression.IfThenElse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")),
									  between == null
										? Expression.Empty()
										: WriteToTextWriter(_currentOutputExp, between),
									  Expression.Block(Expression.Goto(breakTarget))));

			//Primes the loop
			ConditionalExpression outerIf = Expression.IfThen(
				Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")),
				Expression.Loop(loopWrapper, breakTarget));
			loop.Add(outerIf);

			var parameters = node.LoopBody.SymbolTable.GetParameterExpressions().Union(new[] { enumerator }).ToList();

			_loopContextStack.Pop();

			return Expression.Block(parameters, loop);
		}

		Expression AnalyzeGetEnumerator(ForStatement node, Expression enumerable)
		{
			Expression getEnumerator;
			if (node.Where == null)
				getEnumerator = Expression.Call(Expression.Convert(enumerable, typeof(IEnumerable)), typeof(IEnumerable).GetMethod("GetEnumerator"));
			else
			{
				HappySymbolTable whereSymbolTable = new HappySymbolTable("loopWhereClause", node.LoopBody.SymbolTable);
				ParameterExpression whereLambdaParameter = Expression.Parameter(typeof(object), node.LoopVariable.Text);
				whereSymbolTable.Add(whereLambdaParameter);
				Expression whereExpression;
				using (_scopeStack.TrackPush(whereSymbolTable))
					whereExpression = Expression.Convert(this.Analyze(node.Where), typeof(bool));

				Expression getWhereEnumerable = Expression.Call(typeof(RuntimeHelpers), "GetWhereEnumerable", new Type[] { },
																Expression.Convert(enumerable, typeof(IEnumerable)),
																Expression.Lambda(Expression.Block(new[] { whereExpression }),
																				  whereSymbolTable.GetParameterExpressions()));

				getEnumerator = Expression.Call(getWhereEnumerable, "GetEnumerator", new Type[] { });
			}
			return getEnumerator;
		}

		/// <summary>
		/// This only hanldes function calls that are at the global scope, i.e. not in dotted expressions
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		Expression Analyze(FunctionCallExpression node)
		{
			Expression getMethod = this.PropertyOrFieldGet(node.Identifier.Text, _globalScopeExp);
			List<Expression> args = new List<Expression>();
			foreach (ExpressionNodeBase param in node.Arguments)
				args.Add(this.Analyze(param));

			return this.Invoke(getMethod, args);
		}

		Expression Analyze(DefStatement node)
		{
			var initializers = (from varDef in node.VariableDefs
								where varDef.InitializerExpression != null
								select varDef.Symbol.GetSetExpression(this.Analyze(varDef.InitializerExpression))).ToList();

			return initializers.Count > 0 ? Expression.Block(initializers) : null;
		}

		Expression Analyze(ReturnStatement node)
		{
			Expression @return = node.ReturnExp == null ? Expression.Constant(null) : this.Analyze(node.ReturnExp);
			return Expression.Goto(_returnLabelTarget, Expression.Convert(@return, typeof(object)));
		}

		Expression Analyze(NewObjectExpression node)
		{
			var args = node.ConstructorAgs.Select(this.Analyze).ToList();
			args.Insert(0, this.Analyze(node.TypeExpression));
			return Expression.Dynamic(_languageContext.CreateCreateBinder(new CallInfo(node.ConstructorAgs.Length)), typeof(object), args);
		}

		#endregion

		#region Support Methods

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
			var assembliesToLoad = DefaultReferences.Union(loadDirectives.Select(ls => ls.AssemblyName));
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

		void RunAllVisitors(Module module, Dictionary<string, HappyNamespaceTracker> rootNamespaces)
		{
			//Visitors.AstVisitorBase[] visitors =
			//	{
			//		new Visitors.BuildSymbolTablesVisitor(_errorCollector, rootNamespaces, GetGlobalScopeGetter, GetGlobalScopeSetter),
			//		new Visitors.SemanticVisitor(_errorCollector),
			//		new Visitors.PreAnalyzeVisitor(_errorCollector)
			//	};

			//foreach (var v in visitors)
			//	module.Accept(v);
		}

		public HappySymbolTable TopSymbolTable
		{
			get { return _scopeStack.Peek(); }
		}

		public Expression GetGlobalScopeGetter(string globalName)
		{
			return this.PropertyOrFieldGet(globalName, _globalScopeExp);
		}

		public Expression GetGlobalScopeSetter(string globalName, Expression value)
		{
			return this.PropertyOrFieldSet(globalName, _globalScopeExp, value);
		}
		#endregion

		#region Expression Factories

		public static Expression WriteToTextWriter(Expression writerExpression, string text)
		{
			MethodInfo write = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });
			return Expression.Call(writerExpression, write, Expression.Constant(text));
		}

		public static Expression WriteToTextWriter(Expression writerExpression, Expression value)
		{
			MethodInfo write = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });
			MethodInfo toString = typeof(object).GetMethod("ToString");

			if (value.NodeType == ExpressionType.Constant || value.Type.IsPrimitive)
			{
				if (value.Type != typeof(string))
					value = Expression.Call(value, toString);

				return Expression.Call(writerExpression, write, value);
			}

			ParameterExpression tmp = Expression.Parameter(typeof(object), "tmp");
			Expression convertedValue = Expression.Call(tmp, toString);
			return
				Expression.Block(new[] { tmp },
								 Expression.Assign(tmp, Expression.Convert(value, typeof(object))),
								 Expression.IfThen(Expression.NotEqual(tmp, Expression.Constant(null)),
												   Expression.Call(writerExpression, write, convertedValue)));

		}

		public Expression PushWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PushWriter");
			return Expression.Call(Expression.Convert(_runtimeContextExp, typeof(HappyRuntimeContext)), push);
		}

		public Expression PopWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PopWriter");
			return Expression.Call(Expression.Convert(_runtimeContextExp, typeof(HappyRuntimeContext)), push);

		}

		DynamicExpression PropertyOrFieldGet(string name, Expression instance)
		{
			return Expression.Dynamic(_languageContext.CreateGetMemberBinder(name, false), typeof(object), instance);
		}

		DynamicExpression PropertyOrFieldSet(string name, Expression instance, Expression newValue)
		{
			return Expression.Dynamic(_languageContext.CreateSetMemberBinder(name, false), typeof(object), instance, newValue);
		}

		DynamicExpression Invoke(Expression funcExpression, List<Expression> argList)
		{
			List<Expression> newArgList = new List<Expression> { funcExpression };
			argList.ForEach(newArgList.Add);

			return Expression.Dynamic(
				_languageContext.CreateInvokeBinder(new CallInfo(argList.Count)),
				typeof(object),
				newArgList);
		}
		#endregion
	}
}

