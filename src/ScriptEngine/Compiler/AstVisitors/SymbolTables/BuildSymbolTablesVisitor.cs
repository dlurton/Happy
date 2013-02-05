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
using Happy.ScriptEngine.Exceptions;
using Happy.ScriptEngine.Runtime.Trackers;

namespace Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables
{
	class BuildSymbolTablesVisitor : AstVisitorBase
	{
		static readonly string[] _defaultUseNamespaces = new[]
		{
			"Happy.RuntimeLib"
		};

		public static string[] DefaultUseNamespaces { get { return _defaultUseNamespaces; } }

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
		bool IsInGlobalScope { get { return _currentModule.GetExtension<ScopeExtension>().SymbolTable == _scopeStack.Peek(); } }
		bool IsInImportScope { get { return _currentModule.UseStatements.GetExtension<ScopeExtension>().SymbolTable == _scopeStack.Peek(); } }

		public override void BeforeVisit(StatementBlock node)
		{
			node.GetExtension<ScopeExtension>().SymbolTable = new HappySymbolTable("Block", this.TopSymbolTable);
			_scopeStack.Push(node.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void AfterVisit(StatementBlock node)
		{
			PopAndAssert(node.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void BeforeVisit(UseStatementList node)
		{
			base.BeforeVisit(node);
			_scopeStack.Push(node.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void AfterVisit(UseStatementList node)
		{
			base.AfterVisit(node);

			PopAndAssert(node.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void BeforeVisit(Module node)
		{
			_currentModule = node;

			node.UseStatements.GetExtension<ScopeExtension>().SymbolTable = new HappySymbolTable("UseStatementList (Import Scope)", null);
			node.GetExtension<ScopeExtension>().SymbolTable = new HappySymbolTable("Globals", node.UseStatements.GetExtension<ScopeExtension>().SymbolTable);
			
			_scopeStack.Push(node.UseStatements.GetExtension<ScopeExtension>().SymbolTable);
			_scopeStack.Push(node.GetExtension<ScopeExtension>().SymbolTable);

			foreach(var ns in _defaultUseNamespaces)
			{
				string[] namespaceSegments = ns.Split('.');
				addTrackerForDeepestNamespaceToModuleSymbolTable(namespaceSegments, 
				                                                 s =>
				                                                 {
					                                                 throw new InternalException("Failed to load default namespace: {0}", ns);
				                                                 });
			}

			foreach (HappyNamespaceTracker tracker in _rootNamespaces.Values)
			{
				node.GetExtension<ScopeExtension>().SymbolTable.Add(new HappyNamedExpressionSymbol(tracker.Name, _getGlobalGetter));
			}

		}

		public override void AfterVisit(Module node)
		{
			PopAndAssert(node.GetExtension<ScopeExtension>().SymbolTable);
			PopAndAssert(node.UseStatements.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void BeforeVisit(Function node)
		{
			base.BeforeVisit(node);
			node.ParameterList.GetExtension<ScopeExtension>().SymbolTable = new HappySymbolTable("Function " + node.Name.Text, this.TopSymbolTable);
			_scopeStack.Push(node.ParameterList.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void AfterVisit(Function node)
		{
			DebugAssert.AreEqual(node.ParameterList.GetExtension<ScopeExtension>().SymbolTable, _scopeStack.Pop(), "Item at top of scope stack was not the current node");
			base.AfterVisit(node);
		}

		public override void BeforeVisit(VariableDef def)
		{
			base.BeforeVisit(def);

			DebugAssert.IsFalse(this.IsInImportScope, "Cannot be in import scope while building symbols for a VariableDef");

			if(_scopeStack.Peek().Items.ContainsKey(def.Name.Text))
				_errorCollector.VariableAlreadyDefined(def.Name);

			if(this.IsInGlobalScope) 
				def.GetExtension<SymbolExtension>().Symbol = _scopeStack.Peek().Add(new HappyNamedExpressionSymbol(def.Name.Text, _getGlobalGetter, _setGlobalGetter));
			else 
				def.GetExtension<SymbolExtension>().Symbol = _scopeStack.Peek().Add(def.Name.Text);
		}

		public override void Visit(FunctionParameter node)
		{
			base.Visit(node);
			if(this.TopSymbolTable.Items.ContainsKey(node.Name.Text))
				_errorCollector.DuplicateFunctionParameterName(node.Name);
			else
				node.GetExtension<SymbolExtension>().Symbol = this.TopSymbolTable.Add(node.Name.Text);
		}

		public override void AfterVisit(ForStatement node)
		{
			base.AfterVisit(node);
			if(node.LoopBody.GetExtension<ScopeExtension>().SymbolTable.FindInCurrentScope(node.LoopVariable.Text) != null)
				_errorCollector.VariableOfSameNameDefinedInForLoopScope(node.LoopVariable);
			else
				node.GetExtension<ForExtension>().LoopVariableSymbol = node.LoopBody.GetExtension<ScopeExtension>().SymbolTable.Add(node.LoopVariable.Text);
		}

		public override void BeforeVisit(ForWhereClause node)
		{
			var whereSymbolTable = new HappySymbolTable("where(" + node.LoopVariableName + ")", this.TopSymbolTable);
			whereSymbolTable.Add(node.LoopVariableName);
			node.GetExtension<ScopeExtension>().SymbolTable = whereSymbolTable;
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
			addTrackerForDeepestNamespaceToModuleSymbolTable(node.NamespaceSegments, invalidNamespaceSegment => _errorCollector.UseStatementDoesNotEvaluateToANamespace(node, invalidNamespaceSegment));
		}
		
		void addTrackerForDeepestNamespaceToModuleSymbolTable(string[] namespaceSegments, Action<string> errorAction)
		{
			HappyNamespaceTracker currentTracker;
			if (!_rootNamespaces.TryGetValue(namespaceSegments[0], out currentTracker))
				DebugAssert.Fail();

			foreach (string segment in namespaceSegments.Skip(1))
			{
				IHappyTracker nextTracker;
				if (currentTracker.TryGetMember(segment, out nextTracker))
				{
					currentTracker = nextTracker as HappyNamespaceTracker;

					if (currentTracker == null)
					{
						errorAction(segment);
						return;
					}
				}
			}
			foreach (IHappyTracker tracker in currentTracker)
			{
				if (_currentModule.GetExtension<ScopeExtension>().SymbolTable.ExistsInCurrentScope(tracker.Name))
					throw new InternalException("Naming conflict detected involving {0}.{1}.  " +
					                            "It's on the author's TODO list to come up a way of handling this scenario.  In the mean time, don't 'use' this namespace and use fully qualified type names intead.",
					                            currentTracker.FullName, tracker.Name);

				_currentModule.GetExtension<ScopeExtension>().SymbolTable.Add(new HappyNamedExpressionSymbol(tracker.Name, _getGlobalGetter));
			};
		}
	}
}