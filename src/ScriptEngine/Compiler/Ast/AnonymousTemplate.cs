/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class AnonymousTemplate : ExpressionNodeBase
	{
		public StatementBlock Body { get; private set; }

		internal override AstNodeKind NodeKind { get { return AstNodeKind.AnonymousTemplate; } }

		public AnonymousTemplate(StatementBlock body)
			: base(body.Location)
		{
			this.Body = body;
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				this.Body.Accept(visitor);

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			using(writer.Block("<|", "|>"))
				writer.Write(this.Body);
		}
	}
}

