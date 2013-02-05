/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class ForWhereClause : ScopeAstNodeBase
	{
		public string LoopVariableName { get; private set; }
		readonly ExpressionNodeBase _expressionNode;
		public ForWhereClause(HappySourceLocation startsAt, string loopVariableName, ExpressionNodeBase expressionNode) : base(startsAt, expressionNode.Location)
		{
			LoopVariableName = loopVariableName;
			_expressionNode = expressionNode;
		}

		internal override AstNodeKind NodeKind { get { return AstNodeKind.ForWhereClause; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if(_expressionNode != null)
				_expressionNode.Accept(visitor);
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			if (_expressionNode != null)
			{
				writer.Write("where ");
				writer.Write(_expressionNode);
			}
		}
	}
}

