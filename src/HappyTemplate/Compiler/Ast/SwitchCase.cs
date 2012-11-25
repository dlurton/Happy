/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class SwitchCase : AstNodeBase
	{
		public ExpressionNodeBase[] CaseValues{ get; private set; }
		public StatementBlock CaseStatementBlock { get; private set; }

		public SwitchCase(ExpressionNodeBase[] caseValues, StatementBlock statements)
			: base(statements.Location)
		{
			this.CaseValues = caseValues;
			this.CaseStatementBlock = statements;
		}

		internal override AstNodeKind NodeKind
		{
			get
			{
				return AstNodeKind.SwitchCase;
			}
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				this.CaseValues.ForAll(cv => cv.Accept(visitor));
				this.CaseStatementBlock.Accept(visitor);
			}

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("case ");
			using(writer.CurlyBraces())
			{
				this.CaseValues.ForAllBetween(cv => cv.WriteString(writer), () => writer.Write(", "));
				writer.WriteLine(":");
				this.CaseStatementBlock.WriteString(writer);
			}
		}
	}
}

