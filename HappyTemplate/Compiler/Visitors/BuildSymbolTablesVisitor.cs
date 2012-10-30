using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Exceptions;
using HappyTemplate.Runtime.Trackers;

namespace HappyTemplate.Compiler.Visitors
{
	class BuildSymbolTablesVisitor : AstVisitorBase
	{
		readonly Stack<HappySymbolTable> _scopeStack = new Stack<HappySymbolTable>();
		readonly Func<string, Expression> _getGlobalGetter;
		readonly Func<string, Expression, Expression> _setGlobalGetter;
		readonly Dictionary<string, HappyNamespaceTracker> _rootNamespaces;
		readonly ErrorCollector _errorCollector;

		Module _currentModule;

		public BuildSymbolTablesVisitor(IGlobalScopeHelper helper, ErrorCollector errorCollector, Dictionary<string, HappyNamespaceTracker> rootNamespaces)
			: base(VisitorMode.VisitNodeAndChildren)
		{
			_errorCollector = errorCollector;
			_getGlobalGetter = helper.GetGlobalScopeGetter;
			_setGlobalGetter = helper.GetGlobalScopeSetter;
			_rootNamespaces = rootNamespaces;
		}

		void PopAndAssert(HappySymbolTable st)
		{
			DebugAssert.AreEqual(st, _scopeStack.Pop(), "Item at top of scope stack was not the current node");
		}

		HappySymbolTable TopSymbolTable { get { return _scopeStack.Count != 0 ? _scopeStack.Peek() : null; } }
		bool IsInGlobalScope { get { return _currentModule.SymbolTable == _scopeStack.Peek(); } }
		bool IsInImportScope { get { return _currentModule.UseStatements.SymbolTable == _scopeStack.Peek(); } }

		public override void BeforeVisit(StatementBlock node)
		{
			node.SymbolTable = new HappySymbolTable("Block", this.TopSymbolTable);
			_scopeStack.Push(node.SymbolTable);
		}

		public override void AfterVisit(StatementBlock node)
		{
			PopAndAssert(node.SymbolTable);
		}

		public override void BeforeVisit(UseStatementList node)
		{
			base.BeforeVisit(node);
			_scopeStack.Push(node.SymbolTable);
		}

		public override void AfterVisit(UseStatementList node)
		{
			base.AfterVisit(node);

			PopAndAssert(node.SymbolTable);
		}

		public override void BeforeVisit(Module node)
		{
			_currentModule = node;
			node.UseStatements.SymbolTable = new HappySymbolTable("UseStatementList (Import Scope)", null);
			node.SymbolTable = new HappySymbolTable("Globals", node.UseStatements.SymbolTable);
			
			_scopeStack.Push(node.UseStatements.SymbolTable);
			_scopeStack.Push(node.SymbolTable);

			foreach (HappyNamespaceTracker tracker in _rootNamespaces.Values)
			{
				var tmpTracker = tracker;
				node.SymbolTable.Add(new HappyNamedExpressionSymbol(tracker.Name, name => Expression.Constant(tmpTracker)));
			}
		}

		public override void AfterVisit(Module node)
		{
			PopAndAssert(node.SymbolTable);
			PopAndAssert(node.UseStatements.SymbolTable);
		}

		public override void BeforeVisit(Function node)
		{
			base.BeforeVisit(node);
			node.ParameterList.SymbolTable = new HappySymbolTable("Function " + node.Name.Text, this.TopSymbolTable);
			_scopeStack.Push(node.ParameterList.SymbolTable);
		}

		public override void AfterVisit(Function node)
		{
			DebugAssert.AreEqual(node.ParameterList.SymbolTable, _scopeStack.Pop(), "Item at top of scope stack was not the current node");
			base.AfterVisit(node);
		}

		public override void BeforeVisit(VariableDef def)
		{
			base.BeforeVisit(def);

			DebugAssert.IsFalse(this.IsInImportScope, "Cannot be in import scope while building symbols for a VariableDef");

			if(_scopeStack.Peek().Items.ContainsKey(def.Name.Text))
				_errorCollector.VariableAlreadyDefined(def.Name);

			if(this.IsInGlobalScope) 
				def.Symbol = _scopeStack.Peek().Add(new HappyNamedExpressionSymbol(def.Name.Text, _getGlobalGetter, _setGlobalGetter));
			else 
				def.Symbol = _scopeStack.Peek().Add(def.Name.Text);
		}

		public override void Visit(FunctionParameter node)
		{
			base.Visit(node);
			if(this.TopSymbolTable.Items.ContainsKey(node.Name.Text))
				_errorCollector.DuplicateFunctionParameterName(node.Name);
			else
				node.Symbol = this.TopSymbolTable.Add(node.Name.Text);
		}

		public override void AfterVisit(ForStatement node)
		{
			base.AfterVisit(node);
			if(node.LoopBody.SymbolTable.FindInCurrentScope(node.LoopVariable.Text) != null)
				_errorCollector.VariableOfSameNameDefinedInForLoopScope(node.LoopVariable);
			else
				node.LoopVariableSymbol = node.LoopBody.SymbolTable.Add(node.LoopVariable.Text);
		}

		public override void BeforeVisit(ForWhereClause node)
		{
			var whereSymbolTable = new HappySymbolTable("where(" + node.LoopVariableName + ")", this.TopSymbolTable);
			whereSymbolTable.Add(node.LoopVariableName);
			node.SymbolTable = whereSymbolTable;
			_scopeStack.Push(whereSymbolTable);
			base.BeforeVisit(node);
		}

		public override void AfterVisit(ForWhereClause node)
		{
			_scopeStack.Pop();
			base.AfterVisit(node);
		}

		public override void Visit(UseStatement node)
		{
			DebugAssert.IsTrue(this.IsInImportScope, "Use statement processed while not in import scope");

			//Find the deepest namepspace;
			HappyNamespaceTracker currentTracker;
			if(!_rootNamespaces.TryGetValue(node.NamespaceSegments[0], out currentTracker))
				DebugAssert.Fail();

			foreach(string segment in node.NamespaceSegments.Skip(1))
			{
				IHappyTracker nextTracker;
				if(currentTracker.TryGetMember(segment, out nextTracker))
				{
					currentTracker = nextTracker as HappyNamespaceTracker;

					if(currentTracker == null)
					{
						_errorCollector.UseStatementDoesNotEvaluateToANamespace(node, segment);
						return;
					}
				}
			}
			node.NamespaceTracker = currentTracker;

			foreach(var tracker in currentTracker)
			{
				if(this.TopSymbolTable.ExistsInCurrentScope(tracker.Name))
					throw new InternalSourceException(node.Location, "Naming conflict detected involving {0}.{1}", currentTracker.FullName,  tracker.Name);

				var tmpTracker = tracker;
				this.TopSymbolTable.Add(new HappyNamedExpressionSymbol(tracker.Name, (name) => Expression.Constant(tmpTracker)));
			}
		}
	}
}



//TODO:  consider refactoring this so we're not modifying the AST tree to support a particular visitor
//TODO:  can we use something like a BuildSymbolTablesContext instead?
namespace HappyTemplate.Compiler.Ast
{
	abstract partial class ScopeAstNodeBase
	{
		HappySymbolTable _symbolTable;
		internal HappySymbolTable SymbolTable
		{
			get
			{
				return _symbolTable;
			}
			set
			{
				DebugAssert.IsNull(_symbolTable, "SymbolTable may only be set once.");
				_symbolTable = value;
			}
		}
	}

	partial class NamedAstNodeBase
	{
		HappySymbolBase _symbol;
		internal HappySymbolBase Symbol
		{
			get
			{
				return _symbol;
			}
			set
			{
				DebugAssert.IsNull(_symbol, "Symbol may only be set once.");
				_symbol = value;
			}
		}
	}

	partial class ForStatement
	{
		HappyParameterSymbol _loopVariableSymbol;
		internal HappyParameterSymbol LoopVariableSymbol
		{
			get
			{
				return _loopVariableSymbol;
			}
			set
			{
				DebugAssert.IsNull(_loopVariableSymbol, "LoopVariableExpression may only be set once.");
				_loopVariableSymbol = value;
			}
		}
	}

	partial class UseStatement
	{
		HappyNamespaceTracker _namespaceTracker;
		internal HappyNamespaceTracker NamespaceTracker
		{
			//get
			//{
			//    return _namespaceTracker;
			//}
			set
			{
				DebugAssert.IsNull(_namespaceTracker, "NamespaceTracker may only be set once.");
				_namespaceTracker = value;
			}
		}
	}
}
