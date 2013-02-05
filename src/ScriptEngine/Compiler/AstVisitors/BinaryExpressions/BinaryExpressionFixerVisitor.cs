/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.Ast;

namespace Happy.ScriptEngine.Compiler.AstVisitors.BinaryExpressions
{
	class BinaryExpressionFixerVisitor : AstVisitorBase
	{
		public BinaryExpressionFixerVisitor() : base(VisitorMode.VisitNodeAndChildren) { }

		public override void BeforeVisit(BinaryExpression node)
		{
			base.BeforeVisit(node);
			if (node.Operator.Operation == OperationKind.Assign)
				node.LeftValue.GetExtension<BinaryExpressionExtension>().AccessType = ExpressionAccessType.Write;

			if (node.Operator.Operation == OperationKind.MemberAccess)
				node.RightValue.GetExtension<NamedExpressionNodeExtension>().IsMemberReference = true;
		}
	}
}

