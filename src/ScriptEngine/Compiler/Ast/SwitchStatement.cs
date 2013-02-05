/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class SwitchStatement : StatementNodeBase
	{
		public SwitchCase[] Cases { get; private set; }
		public ExpressionNodeBase SwitchExpression { get; private set; }
		public StatementBlock DefaultStatementBlock { get; private set; }

		public SwitchStatement(HappySourceLocation startsAt, HappySourceLocation endsAt, ExpressionNodeBase switchExpression, SwitchCase[] cases, StatementBlock defaultCase)
			: base(startsAt, endsAt)
		{
			this.SwitchExpression = switchExpression;
			this.Cases = cases;
			this.DefaultStatementBlock = defaultCase;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.SwitchStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				this.SwitchExpression.Accept(visitor);
				this.Cases.ExecuteOverAll(c => c.Accept(visitor));
				if (this.DefaultStatementBlock != null)
					this.DefaultStatementBlock.Accept(visitor);
			}

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("switch");
			using(writer.CurlyBraces())
			{
				this.SwitchExpression.WriteString(writer);
				this.Cases.ForAllBetween(c => c.WriteString(writer), () => writer.Write("\n"));
				if(this.DefaultStatementBlock != null)
				{
					writer.WriteLine("default:");
					this.DefaultStatementBlock.WriteString(writer);
				}
			}

		}
	}
}

