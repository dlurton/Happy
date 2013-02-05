/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	internal class VerbatimSection : StatementNodeBase
	{
		readonly string _text;

		internal override AstNodeKind NodeKind { get { return AstNodeKind.VerbatimSection; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("verbatim length: {0}, startsAt: ({1})>", _text.Length, this.Location);
		}

		public string Text { get { return _text; } }

		public VerbatimSection(HappySourceLocation span, string output)
			: base(span)
		{
			_text = output;
		}
	}
}

