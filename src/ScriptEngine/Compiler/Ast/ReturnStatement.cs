/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class ReturnStatement : StatementNodeBase
	{
		public ExpressionNodeBase ReturnExp { get; private set; }
		public ReturnStatement(HappySourceLocation location, ExpressionNodeBase returnExp)
			: base(location, returnExp != null ? returnExp.Location : location)
		{
			this.ReturnExp = returnExp;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.ReturnStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (!visitor.ShouldVisitChildren)
				return;

			if(this.ReturnExp != null)
				this.ReturnExp.Accept(visitor);
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("return ");
			if(this.ReturnExp != null)
				using(writer.Indent())
					writer.Write(this.ReturnExp);
		}
	}
}

