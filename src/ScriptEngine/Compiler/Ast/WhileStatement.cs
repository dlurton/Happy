/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class WhileStatement : LoopStatementBase
	{
		public ExpressionNodeBase Condition { get; private set; }
		public StatementBlock Block { get; private set; }
		public WhileStatement(HappySourceLocation startsAt, ExpressionNodeBase condition, StatementBlock block) : base(startsAt, block.Location)
		{
			Block = block;
			this.Condition = condition;
		}

		internal override AstNodeKind NodeKind { get { return AstNodeKind.WhileStatement; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			this.Condition.Accept(visitor);
			this.Block.Accept(visitor);
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}

