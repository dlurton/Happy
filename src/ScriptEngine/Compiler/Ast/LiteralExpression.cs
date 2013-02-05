/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class LiteralExpression : ExpressionNodeBase
	{
		private readonly object _value;

		public object Value { get { return _value; } }
		internal override AstNodeKind NodeKind { get { return AstNodeKind.LiteralExpression; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			if(_value is string)
				writer.WriteLine("\"{0}\"", _value);
			else
				writer.WriteLine("{1}, {0}", _value.GetType(), _value.ToString());
		}

		public LiteralExpression(HappySourceLocation span, object value)
			: base(span)
		{
			_value = value;
		}
	}
}

