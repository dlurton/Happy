/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using Happy.ScriptEngine.Compiler.Ast;

namespace Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables
{
	class ScopedAstVisitorBase : AstVisitorBase
	{
		protected ScopedAstVisitorBase(VisitorMode intitialMode) : base(intitialMode)
		{
			
		}

		readonly Stack<HappySymbolTable> _scopeStack = new Stack<HappySymbolTable>();
		protected HappySymbolTable TopSymbolTable { get { return _scopeStack.Peek(); } }

		public override void BeforeVisitCatchAll(AstNodeBase node)
		{
			base.BeforeVisitCatchAll(node);
			ScopeAstNodeBase scopeNode = node as ScopeAstNodeBase;
			if(scopeNode != null)
				_scopeStack.Push(scopeNode.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void AfterVisitCatchAll(AstNodeBase node)
		{
			ScopeAstNodeBase scopeNode = node as ScopeAstNodeBase;
			if (scopeNode != null)
				_scopeStack.Pop();
			base.AfterVisitCatchAll(node);
		}


		public override void BeforeVisit(ForStatement node)
		{
			base.BeforeVisit(node);			
			_scopeStack.Push(node.LoopBody.GetExtension<ScopeExtension>().SymbolTable);
		}

		public override void AfterVisit(ForStatement node)
		{
			_scopeStack.Pop();
			base.AfterVisit(node);
		}
	}
}

