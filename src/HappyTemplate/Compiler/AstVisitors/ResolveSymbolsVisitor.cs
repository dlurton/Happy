/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler.AstVisitors
{
	class ResolveSymbolsVisitor : ScopedAstVisitorBase
	{
		readonly ErrorCollector _errorCollector;
		public ResolveSymbolsVisitor(ErrorCollector errorCollector) : base(VisitorMode.VisitNodeAndChildren)
		{
			_errorCollector = errorCollector;
		}

		public override void BeforeVisit(BinaryExpression node)
		{
			base.BeforeVisit(node);

			if(node.Operator.Operation == OperationKind.MemberAccess)
				((NamedExpressionNodeBase)node.RightValue).SetIsMemberReference(true);
		}

		public override void Visit(IdentifierExpression node)
		{
			base.Visit(node);

			if (node.GetIsMemberReference())
				return;

			var searchResult = base.TopSymbolTable.FindInScopeTree(node.Identifier.Text);

			if (searchResult == null)
				_errorCollector.UndefinedVariable(node.Identifier);

			node.SetSymbol(searchResult);
		}

	}
}

