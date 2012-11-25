/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	partial class StatementBlock : ScopeAstNodeBase
	{
		public AstNodeBase[] Statements { get; private set; }
		public StatementBlock(HappySourceLocation startsAt, HappySourceLocation endsAt, AstNodeBase[] stmts)
			: base(startsAt, endsAt)
		{
			this.Statements = stmts;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.BlockStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				this.Statements.ExecuteOverAll(n => n.Accept(visitor));
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			using(writer.CurlyBraces())
				this.Statements.ExecuteOverAll(writer.Write);
		}

		public AstNodeBase this[int i]
		{
			get
			{
				return this.Statements[i];
			}
		}

	}
}

