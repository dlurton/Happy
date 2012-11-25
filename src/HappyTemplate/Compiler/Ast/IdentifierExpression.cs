/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	internal class IdentifierExpression : NamedExpressionNodeBase
	{
		internal override AstNodeKind NodeKind { get { return AstNodeKind.IdentifierExpression; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("[{0}]", this.Identifier.Text);
		}

		public IdentifierExpression(Identifier identifier) : base(identifier)
		{
			
		}
	}
}

