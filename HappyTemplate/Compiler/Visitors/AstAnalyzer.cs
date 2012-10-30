using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime;
using HappyTemplate.Runtime.Trackers;
using Microsoft.Scripting;
using BinaryExpression = HappyTemplate.Compiler.Ast.BinaryExpression;
using Module = HappyTemplate.Compiler.Ast.Module;
using SwitchCase = HappyTemplate.Compiler.Ast.SwitchCase;

namespace HappyTemplate.Compiler.Visitors
{
	class AstAnalyzer : ScopedAstVisitorBase, IGlobalScopeHelper
	{
		const string GlobalObjectName = "__global__";
		const string CurrentOutputIdentifier = "__currentOutput__";
		const string RuntimeContextIdentifier = "__runtimeContext__";
		const string EnumeratorParameterExpressionName = "__enumerator__";

		static readonly string[] DefaultReferences = new[]
		{
			"mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
			"System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
		};


		readonly ErrorCollector _errorCollector;
		readonly Stack<Expression> _expressionStack = new Stack<Expression>();
		readonly ParameterExpression _globalScopeExp;
		ParameterExpression _currentOutputExp;
		readonly Expression _runtimeContextExp;
		readonly HappyLanguageContext _languageContext;
		readonly PropertyInfo _outputWriterProperty;
		LabelTarget _returnLabelTarget;
		readonly Stack<LoopContext> _loopContextStack = new Stack<LoopContext>();


		class LoopContext
		{
			public LabelTarget BreakLabel;
			public LabelTarget ContinueLabel;
		}


		public AstAnalyzer(HappyLanguageContext languageContext) : base(VisitorMode.VisitNodeAndChildren)
		{
			_languageContext = languageContext;
			_outputWriterProperty = typeof (HappyRuntimeContext).GetProperty("OutputWriter");
			_errorCollector = new ErrorCollector(languageContext.ErrorSink);
			_globalScopeExp = Expression.Parameter(typeof(IDynamicMetaObjectProvider), GlobalObjectName);
			_runtimeContextExp = Expression.Convert(this.PropertyOrFieldGet(RuntimeContextIdentifier, _globalScopeExp), typeof(HappyRuntimeContext));
		}

		/// <summary>
		/// This method loads the default assemblies and the assemblies specified in loadStatements.
		/// </summary>
		/// <param name="loadDirectives"></param>
		/// <returns>
		///	A dictionary containing the HappyNamespaceTrackers corresponding to the root namespaces in the loaded assemblies.
		/// </returns>
		Dictionary<string, HappyNamespaceTracker> LoadAllAssemblies(IEnumerable<LoadDirective> loadDirectives)
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

		public HappyScriptCode Analyze(Module module, SourceUnit sourceUnit)
		{

			var rootNamespaces = LoadAllAssemblies(module.LoadDirectives);

			AstVisitorBase[] visitors =
				{
					new PreAnalyzeVisitor(),
					new BuildSymbolTablesVisitor(this, _errorCollector, rootNamespaces),
					new ResolveSymbolsVisitor(_errorCollector), 
					new SemanticVisitor(_errorCollector)
				};

			foreach (var v in visitors)
				module.Accept(v);

			module.Accept(this);

			Expression expression = _expressionStack.Pop();

			DebugAssert.IsZero(_expressionStack.Count, "AstAnalyzer didn't consume all expressions on the stack");
			//DebugAssert.IsInstanceOfType(typeof (LambdaExpression), expression);

			var globalScopeInitializer = (LambdaExpression)expression;
			return new HappyScriptCode(sourceUnit, globalScopeInitializer.Compile());
		}

		public override void AfterVisit(Module node)
		{
			//body contains a list of expressions, that when compiled, populate a single expando object
			//with the functions and default values of the global variables.
			List<Expression> body = new List<Expression>();

			body.AddRange(_expressionStack.Pop(node.Functions.Length));
			body.AddRange(_expressionStack.Pop(node.GlobalDefStatements.Length));
			//body.AddRange(_expressionStack.Pop(node.LoadDirectives.Length));

			//Add an empty expression--prevents an exception by Expression.Lambda when body is empty.
			//This allows compilation of empty template sets.
			body.Add(Expression.Empty());

			_expressionStack.Push(Expression.Lambda(typeof (Action<IDynamicMetaObjectProvider>), Expression.Block(body), new[] { _globalScopeExp }));
		}

		public override void Visit(VerbatimSection node)
		{
			base.Visit(node);
			_expressionStack.Push(WriteConstantToTextWriter(_currentOutputExp, node.Text));
		}

		public override void Visit(FunctionParameter node)
		{
			var parameterSymbol = (HappyParameterSymbol)node.Symbol;
			_expressionStack.Push(parameterSymbol.Parameter);
		}

		public override void BeforeVisit(Function node)
		{
			base.BeforeVisit(node);
			_currentOutputExp = Expression.Parameter(typeof (TextWriter), CurrentOutputIdentifier);
			//Create return target
			_returnLabelTarget = Expression.Label(typeof (object), "lambdaReturn");
		}

		public override void AfterVisit(Function node)
		{
			Expression expression = _expressionStack.Pop(); //Should be a BlockExpression
			var parameters = _expressionStack.Pop(node.ParameterList.Count).Cast<ParameterExpression>().ToList();

			List<Expression> outerExpressions = new List<Expression>
			{
				Expression.Assign(_currentOutputExp, Expression.Property(_runtimeContextExp, _outputWriterProperty))
			};

			LabelExpression returnLabel = Expression.Label(_returnLabelTarget, Expression.Default(typeof(object)));

			outerExpressions.Add(expression); 
			outerExpressions.Add(returnLabel);

			BlockExpression completeFunctionBody = Expression.Block(new[] { _currentOutputExp }, outerExpressions);
			LambdaExpression funcLambda = Expression.Lambda(completeFunctionBody, node.Name.Text, parameters);
			_expressionStack.Push(this.PropertyOrFieldSet(node.Name.Text, _globalScopeExp, funcLambda));
			base.AfterVisit(node);
		}

		public override void AfterVisit(StatementBlock node)
		{
			List<Expression> expressions = _expressionStack.Pop(node.Statements.Length);
			if (expressions.Count == 0)
				expressions.Add(Expression.Empty());

			if (node.GetAnalyzeSymbolsExternally())
				_expressionStack.Push(Expression.Block(expressions));
			else
				_expressionStack.Push(Expression.Block(node.SymbolTable.GetParameterExpressions(), expressions));
			base.AfterVisit(node);
		}

		#region Statements
		public override void AfterVisit(DefStatement node)
		{
			var initializers = _expressionStack.Pop(node.VariableDefs.Count(vd => vd.InitializerExpression != null)).ToList();
			_expressionStack.Push(initializers.Count == 0 ? Expression.Empty() : (Expression)Expression.Block(initializers));
			base.AfterVisit(node);
		}

		public override void AfterVisit(VariableDef node)
		{
			if(node.InitializerExpression != null)
				_expressionStack.Push(node.Symbol.GetSetExpression(_expressionStack.Pop()));
			base.AfterVisit(node);
		}

		public override void AfterVisit(OutputStatement node)
		{
			var writeExps = _expressionStack.Pop(node.ExpressionsToWrite.Length).Select(e => WriteToTextWriter(_currentOutputExp, e));
			_expressionStack.Push(Expression.Block(writeExps));
			base.AfterVisit(node);
		}

		public override void AfterVisit(ReturnStatement node)
		{
			Expression @return = node.ReturnExp == null ? Expression.Constant(null) : _expressionStack.Pop();
			_expressionStack.Push(Expression.Goto(_returnLabelTarget, Expression.Convert(@return, typeof(object))));
			base.AfterVisit(node);
		}

		public override void AfterVisit(IfStatement node)
		{
			Expression falseBlock = null;

			if (node.FalseStatementBlock != null)
				falseBlock = _expressionStack.Pop();

			Expression trueBlock = _expressionStack.Pop();
			Expression condition =  Expression.Convert(_expressionStack.Pop(), typeof(Boolean));

			ConditionalExpression ifThenExpression;
			if (falseBlock != null)
				ifThenExpression = Expression.IfThenElse(condition, trueBlock, falseBlock);
			else
				ifThenExpression = Expression.IfThen(condition, trueBlock);

			_expressionStack.Push(ifThenExpression);
			base.AfterVisit(node);
		}

		void pushLoopContext()
		{
			LabelTarget breakTarget = Expression.Label("break");
			LabelTarget continueTarget = Expression.Label("continue");

			LoopContext context = new LoopContext { BreakLabel = breakTarget, ContinueLabel = continueTarget };
			_loopContextStack.Push(context);
		}

		public override void BeforeVisit(WhileStatement node)
		{
			base.BeforeVisit(node);
			pushLoopContext();
		}

		public override void AfterVisit(WhileStatement node)
		{
			Expression loopBody = _expressionStack.Pop();
			Expression condition = _expressionStack.Pop();
			var loopContext = _loopContextStack.Pop();

			var loopController = Expression.IfThen(Expression.Not(RuntimeHelpers.EnsureBoolResult(condition)), Expression.Goto(loopContext.BreakLabel));

			var loopExprs = new List<Expression>
			{
				//Expression.Label(loopContext.ContinueLabel), 
				loopController,
				loopBody,
				//Expression.Label(loopContext.BreakLabel)
			};

			_expressionStack.Push(Expression.Loop(Expression.Block(loopExprs), loopContext.BreakLabel, loopContext.ContinueLabel));

			base.AfterVisit(node);
		}

		public override void BeforeVisit(ForStatement node)
		{
			base.BeforeVisit(node);

			pushLoopContext();

			node.LoopBody.SetAnalyzeSymbolsExternally(true);
		}

		public override void AfterVisit(ForStatement node)
		{
			Expression whereCondition = null, between = null, getEnumerator;
			var currentLoopContext = _loopContextStack.Pop();

			if (node.Where != null)
				whereCondition = _expressionStack.Pop();

			if(node.Between != null)
				between = _expressionStack.Pop();

			BlockExpression loopBody = _expressionStack.PopCast<BlockExpression>();
			Expression enumerable = _expressionStack.Pop();

			ParameterExpression loopVariable = node.LoopVariableSymbol.GetGetExpression() as ParameterExpression;
			getEnumerator = whereCondition == null 
				? Expression.Call(Expression.Convert(enumerable, typeof(IEnumerable)), typeof(IEnumerable).GetMethod("GetEnumerator")) 
				: getWhereEnumerator(node, enumerable, whereCondition);
			
			ParameterExpression enumerator = Expression.Parameter(typeof(IEnumerator), EnumeratorParameterExpressionName);
			List<Expression> loop = new List<Expression> { Expression.Assign(enumerator, getEnumerator) };

			//The loop "wrapper", which wraps the actual loop body
			//It's purpose is to assign the loopVariable after each iteration and 
			//to write the between clause to the current output 
			BlockExpression loopWrapper = Expression.Block(
				Expression.Assign(loopVariable, Expression.Property(enumerator, typeof(IEnumerator).GetProperty("Current"))),
				loopBody,
				Expression.Label(currentLoopContext.ContinueLabel),
				Expression.IfThenElse(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")),
									  between == null
										? Expression.Empty()
										: WriteToTextWriter(_currentOutputExp, between),
									  Expression.Block(Expression.Goto(currentLoopContext.BreakLabel))));

			//Primes the loop
			ConditionalExpression outerIf = Expression.IfThen(
				Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")),
				Expression.Loop(loopWrapper, currentLoopContext.BreakLabel));
			loop.Add(outerIf);

			var parameters = node.LoopBody.SymbolTable.GetParameterExpressions().Union(new[] { enumerator }).ToList();

			_expressionStack.Push(Expression.Block(parameters, loop));

			base.AfterVisit(node);
		}

		public override void AfterVisit(ForWhereClause node)
		{
			var whereCondition = _expressionStack.Pop();
			_expressionStack.Push(RuntimeHelpers.EnsureBoolResult(whereCondition));
			base.AfterVisit(node);
		}

		static Expression getWhereEnumerator(ForStatement node, Expression enumerable, Expression whereExpression)
		{
			var methodInfo = typeof (RuntimeHelpers).GetMethod("GetWhereEnumerable");
			var arguments = new Expression[]
			{
				Expression.Convert(enumerable, typeof(IEnumerable)), 
				Expression.Lambda(whereExpression, node.Where.SymbolTable.GetParameterExpressions())
			};

			Expression retval = Expression.Call(methodInfo, arguments);
			return Expression.Call(retval, "GetEnumerator", new Type[] { });
		}

		public override void Visit(BreakStatement node)
		{
			base.Visit(node);
			DebugAssert.IsNonZero(_loopContextStack.Count, "break statement within loop");
			_expressionStack.Push(Expression.Break(_loopContextStack.Peek().BreakLabel));
		}

		public override void Visit(ContinueStatement node)
		{
			base.Visit(node);
			DebugAssert.IsNonZero(_loopContextStack.Count, "break statement within loop");
			_expressionStack.Push(Expression.Continue(_loopContextStack.Peek().ContinueLabel));
		}
		public override void AfterVisit(SwitchStatement node)
		{
			//In the case of a switch statement with no cases, the default block always executes and the expression should be evaluated too, in case there are side effects.
			//Note:  Can't pass an empty cases collection to Expression.Switch() - an ArgumentException will result.
			if (node.Cases.Length == 0)
			{
				var notSwitchExprs = new List<Expression>();
				if (node.DefaultStatementBlock != null)
					notSwitchExprs.Add(_expressionStack.Pop());

				notSwitchExprs.Add(_expressionStack.Pop());
				_expressionStack.Push(Expression.Block(notSwitchExprs));
				return;
			}

			Expression defaultBlock = null;
			if(node.DefaultStatementBlock != null)
				defaultBlock = _expressionStack.Pop();
			var cases = _expressionStack.Pop(node.Cases.Length).Cast<ContainerPseudoExpression<System.Linq.Expressions.SwitchCase>>().Select(container => container.ContainedItem).ToList();
			var switchExpr = _expressionStack.Pop();
			MethodInfo comparerMethodInfo = typeof (RuntimeHelpers).GetMethod("HappyEq");

			_expressionStack.Push(Expression.Switch(switchExpr, defaultBlock, comparerMethodInfo, cases));

			base.AfterVisit(node);
		}

		public override void AfterVisit(SwitchCase node)
		{
			var caseBlock = _expressionStack.Pop(); //Should be a block expression
			var caseValues = _expressionStack.Pop(node.CaseValues.Length);
			_expressionStack.Push(new ContainerPseudoExpression<System.Linq.Expressions.SwitchCase>(Expression.SwitchCase(caseBlock, caseValues)));

			base.AfterVisit(node);
		}

		#endregion 

		#region Expressions
		public override void Visit(LiteralExpression node)
		{
			base.Visit(node);
			//TODO:  see if we can impart some intelligence instead of blindly returning typeof(object) here...
			//might get better performance with strongly typed literals.
			_expressionStack.Push(Expression.Constant(node.Value, typeof(object)));
		}

		public override void AfterVisit(Ast.UnaryExpression node)
		{
			switch (node.Operator.Operation)
			{
			case OperationKind.Not:
				_expressionStack.Push(Expression.MakeUnary(ToExpressionType(node.Operator), Expression.Convert(_expressionStack.Pop(), typeof(bool)), typeof(object)));
				break;
			default:
				throw new UnhandledCaseException("Semantic checking should have prevented this unhandled case");
			}
			base.AfterVisit(node);
		}


		public override void AfterVisit(BinaryExpression node)
		{
			var expType = ToExpressionType(node.Operator);
			switch (node.AccessType)
			{
			case ExpressionAccessType.Read:
				analyzeReadAccessExpression(node, expType);
				break;
			//This case is only included as a sanity test
			//analysis for writable expressions are handled by their parent BinaryExpression (the assignment expression itself)
			case ExpressionAccessType.Write:
				switch (node.Operator.Operation)
				{
					//The only writeable expression types are:
					case OperationKind.Index:
					case OperationKind.MemberAccess:
					break;
					default:
						throw new UnhandledCaseException("Expression type " + expType + " is not writable.  (user error?)");
				}
				break;
			default:
				throw new UnhandledCaseException();
			}

			base.AfterVisit(node);
		}

		void analyzeReadAccessExpression(BinaryExpression node, ExpressionType expType)
		{
			//Read operations which do not pop rvalue or lvalues from the stack.
			switch (expType)
			{
			case ExpressionType.Index:
				analyzeReadIndexExpression(node);
				return;
			case ExpressionType.MemberAccess:
				analyzeReadMemberAccess(node);
				return;
			}

			Expression rvalue = _expressionStack.Pop();
			//These operations easily pop their rvalue from _expressionStack but lvalue requires special handling
			if (expType == ExpressionType.Assign)
			{
				analyzeAssignmentExpression(node, rvalue);
				return;
			}

			//All other operations can expect to find both their rvalue and lvalues on the stack.
			Expression lvalue = _expressionStack.Pop();
			switch (expType)
			{
			case ExpressionType.OrElse:
				_expressionStack.Push(Expression.OrElse(RuntimeHelpers.EnsureBoolResult(lvalue), RuntimeHelpers.EnsureBoolResult(rvalue)));
				return;
			case ExpressionType.AndAlso:
				_expressionStack.Push(Expression.AndAlso(RuntimeHelpers.EnsureBoolResult(lvalue), RuntimeHelpers.EnsureBoolResult(rvalue)));
				return;
			default:
				_expressionStack.Push(Expression.Dynamic(_languageContext.CreateBinaryOperationBinder(expType), typeof (object), lvalue, rvalue));
				return;
			}
		}

		void analyzeAssignmentExpression(BinaryExpression node, Expression rvalue)
		{
			switch (node.LeftValue.NodeKind)
			{
			case AstNodeKind.IdentifierExpression:
				var identifierExp = (IdentifierExpression)node.LeftValue;
				_expressionStack.Push(identifierExp.GetSymbol().GetSetExpression(rvalue));
				return;
			case AstNodeKind.BinaryExpression:
				var binaryLvalue = (BinaryExpression)node.LeftValue;
				if (binaryLvalue == null)
					return;
				analyzeAssignmentToBinaryExpression(rvalue, binaryLvalue);
				return;
			default:
				_errorCollector.InvalidLValueForAssignment(node.LeftValue.Location);
				return;
			}
		}

		void analyzeAssignmentToBinaryExpression(Expression rvalue, BinaryExpression binaryLvalue)
		{
			//A BinaryExpression on as the LeftValue can be either an Index or MemberAccess operation
			switch (binaryLvalue.Operator.Operation)
			{
			case OperationKind.Index:
				//Have to handle array index and member assignments here because we don't know the rvalue until now.
				var args = popIndexArguments(binaryLvalue);
				args.Add(rvalue);
				DynamicExpression indexExpression = Expression.Dynamic(_languageContext.CreateSetIndexBinder(new CallInfo(args.Count)), typeof (object), args);
				_expressionStack.Push(indexExpression);
				return;
			case OperationKind.MemberAccess:
				//In this case, the rvalue is the value to be assigned (it does not correspond to node.RightValue) 
				var instanceExpr = _expressionStack.Pop();
				_expressionStack.Push(this.PropertyOrFieldSet(((IdentifierExpression)binaryLvalue.RightValue).Identifier.Text, instanceExpr, rvalue));
				return;
			default:
				_errorCollector.InvalidLValueForAssignment(binaryLvalue.Location);
				break;
			}
		}

		void analyzeReadMemberAccess(BinaryExpression node)
		{
			switch (node.RightValue.NodeKind)
			{
			case AstNodeKind.IdentifierExpression:
				Expression instanceExpr = _expressionStack.Pop();
				var propertyOrFieldGet = this.PropertyOrFieldGet(((IdentifierExpression)node.RightValue).Identifier.Text, instanceExpr);
				_expressionStack.Push(propertyOrFieldGet);
				break;
			case AstNodeKind.FunctionCallExpression:
				FunctionCallExpression funcCallExpression = (FunctionCallExpression)node.RightValue;
				List<Expression> args = _expressionStack.Pop(funcCallExpression.Arguments.Length);
				args.Insert(0, _expressionStack.Pop()); //<--object instance
				_expressionStack.Push(Expression.Dynamic(_languageContext.CreateCallBinder(funcCallExpression.Identifier.Text, false, new CallInfo(args.Count)), typeof(object), args));
				break;
			default:
				throw new UnhandledCaseException();
			}
		}

		void analyzeReadIndexExpression(BinaryExpression node)
		{
			var args = popIndexArguments(node);
			DynamicExpression indexExpression = Expression.Dynamic(_languageContext.CreateGetIndexBinder(new CallInfo(args.Count)), typeof (object), args);
			_expressionStack.Push(indexExpression);
		}

		List<Expression> popIndexArguments(BinaryExpression node)
		{
			ArgumentList argList = (ArgumentList)node.RightValue;
			List<Expression> args = new List<Expression>();
			List<Expression> indexerArguments = _expressionStack.Pop(argList.Arguments.Length);
			Expression array = _expressionStack.Pop();
			args.Add(array);
			args.AddRange(indexerArguments);
			return args;
		}

		public override void Visit(IdentifierExpression node)
		{
			base.Visit(node);
			//Member references must be handled in AfterVisit(BinaryExpression) 
			//because it requires knowledge of the instance which we don't have yet
			//and if it's a write operation, the value being assigned.
			if (node.GetIsMemberReference())
				return;

			//Also handle reading only because Assigning a value to IdentifierExpressions must be 
			//handled in AfterVisit(BinaryExpression) since it requires knowledge of the expression 
			//who's value is to be assigned.
			if (node.AccessType == ExpressionAccessType.Read) 
				_expressionStack.Push(node.GetSymbol().GetGetExpression());
		}

		public override void AfterVisit(AnonymousTemplate node)
		{
			List<Expression> body = new List<Expression>();
			var previousCurrentOutputExp = _currentOutputExp;
			_currentOutputExp = Expression.Parameter(typeof(TextWriter), CurrentOutputIdentifier);

			body.Add(Expression.Assign(_currentOutputExp, PushWriter()));
			body.Add(_expressionStack.Pop());
			body.Add(PopWriter());

			var tmpOutputExpression = _currentOutputExp;
			_currentOutputExp = previousCurrentOutputExp;

			_expressionStack.Push(Expression.Block(node.Body.SymbolTable.GetParameterExpressions().Union(new[] { tmpOutputExpression }), body));
			base.AfterVisit(node);
		}

		/// <summary>
		/// This only hanldes function calls that are at the global scope, i.e. not in dotted expressions.
		/// </summary>
		/// <param name="node"></param>
		public override void AfterVisit(FunctionCallExpression node)
		{
			//Member references are handled during analysis of the parent BinaryExpression node
			//because knowledge of both the instance and member are required.
			if (!node.GetIsMemberReference())
			{
				Expression getMethod = this.PropertyOrFieldGet(node.Identifier.Text, _globalScopeExp);
				List<Expression> args = _expressionStack.Pop(node.Arguments.Length);
				_expressionStack.Push(this.Invoke(getMethod, args));
			}
			base.AfterVisit(node);
		}

		public override void AfterVisit(NewObjectExpression node)
		{
			var args = _expressionStack.Pop(node.ConstructorAgs.Length + 1);
			_expressionStack.Push(Expression.Dynamic(_languageContext.CreateCreateBinder(new CallInfo(node.ConstructorAgs.Length)), typeof(object), args));

			base.AfterVisit(node);
		}

		public override void Visit(NullExpression node)
		{
			_expressionStack.Push(Expression.Constant(null, typeof(object)));
		}
		#endregion

		#region IGlobalScopeHelper 
		public Expression GetGlobalScopeGetter(string globalName)
		{
			return this.PropertyOrFieldGet(globalName, _globalScopeExp);
		}

		public Expression GetGlobalScopeSetter(string globalName, Expression value)
		{
			return this.PropertyOrFieldSet(globalName, _globalScopeExp, value);
		}
		#endregion 

		#region Privates


		class ContainerPseudoExpression<T> : Expression
		{
			public T ContainedItem { get; private set; }
			public ContainerPseudoExpression(T item)
			{
				this.ContainedItem = item;
			}

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

		Expression PushWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PushWriter");
			return Expression.Call(Expression.Convert(_runtimeContextExp, typeof(HappyRuntimeContext)), push);
		}

		Expression PopWriter()
		{
			MethodInfo push = typeof(HappyRuntimeContext).GetMethod("PopWriter");
			return Expression.Call(Expression.Convert(_runtimeContextExp, typeof(HappyRuntimeContext)), push);

		}

		static Expression WriteConstantToTextWriter(Expression writerExpression, string text)
		{
			MethodInfo write = typeof(TextWriter).GetMethod("Write", new[] { typeof(string) });
			return Expression.Call(writerExpression, write, Expression.Constant(text));
		}

		Expression WriteToTextWriter(Expression writerExpression, Expression value)
		{
			MethodInfo mi = typeof (RuntimeHelpers).GetMethod("SafeWriteToTextWriter");
			return Expression.Call(mi, _currentOutputExp, RuntimeHelpers.EnsureObjectResult(value));
		}

		DynamicExpression PropertyOrFieldGet(string name, Expression instance)
		{
			return Expression.Dynamic(_languageContext.CreateGetMemberBinder(name, false), typeof(object), instance);
		}

		DynamicExpression PropertyOrFieldSet(string name, Expression instance, Expression newValue)
		{
			return Expression.Dynamic(_languageContext.CreateSetMemberBinder(name, false), typeof(object), instance, newValue);
		}

		static ExpressionType ToExpressionType(Operator node)
		{
			ExpressionType expType;

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
				case OperationKind.MemberAccess:
					expType = ExpressionType.MemberAccess;
					break;
				default:
					throw new UnhandledCaseException(node.Operation.ToString());
			}
			return expType;
		}
		#endregion
	}
}