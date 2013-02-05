/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.Ast;
using Happy.ScriptEngine.Compiler.AstVisitors.BinaryExpressions;

namespace Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables
{
	class ResolveSymbolsVisitor : ScopedAstVisitorBase
	{
		readonly ErrorCollector _errorCollector;
		public ResolveSymbolsVisitor(ErrorCollector errorCollector) : base(VisitorMode.VisitNodeAndChildren)
		{
			_errorCollector = errorCollector;
		}

		public override void Visit(IdentifierExpression node)
		{
			base.Visit(node);

			if (node.GetExtension<NamedExpressionNodeExtension>().IsMemberReference)
				return;

			var searchResult = base.TopSymbolTable.FindInScopeTree(node.Identifier.Text);

			if (searchResult == null)
				_errorCollector.UndefinedVariable(node.Identifier);

			node.GetExtension<SymbolExtension>().Symbol = searchResult;
		}

	}
}

