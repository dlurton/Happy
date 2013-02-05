/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	internal class FunctionParameter : NamedAstNodeBase
	{
		internal override AstNodeKind NodeKind { get { return AstNodeKind.TemplateParameter; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write(this.Name.Text);
		}

		public FunctionParameter(Identifier name)
			: base(name)
		{
		}
	}
}

