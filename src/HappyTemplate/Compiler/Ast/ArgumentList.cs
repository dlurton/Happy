/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class ArgumentList : ExpressionNodeBase 
	{
		public ExpressionNodeBase[] Arguments { get; private set; }
		public ArgumentList(HappySourceLocation location, ExpressionNodeBase[] arguments)
			: base(location) 
		{
			this.Arguments = arguments;
		}

		internal override AstNodeKind NodeKind  { get { return AstNodeKind.ArgumentList; } 
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren) 
				foreach (ExpressionNodeBase arg in this.Arguments)
					arg.Accept(visitor);

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			using(writer.Parens())
				writer.Write(this.Arguments);
		}
	}
}

