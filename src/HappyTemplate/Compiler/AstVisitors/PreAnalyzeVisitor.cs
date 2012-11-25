/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.Ast;

namespace HappyTemplate.Compiler.AstVisitors
{
	class PreAnalyzeVisitor : AstVisitorBase
	{
		public PreAnalyzeVisitor() : base(VisitorMode.VisitNodeAndChildren) { }

		public override void BeforeVisit(BinaryExpression node)
		{
			base.BeforeVisit(node);
			if (node.Operator.Operation == OperationKind.Assign)
			{
				node.LeftValue.AccessType = ExpressionAccessType.Write;
				return;
			}
		}
	}
}

