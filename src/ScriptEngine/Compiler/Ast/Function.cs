/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class Function : NamedAstNodeBase
	{
		readonly StatementBlock _body;
		readonly FunctionParameterList _parameterList;

		public Function(HappySourceLocation startsAt, Identifier name, FunctionParameterList parameterList, StatementBlock body)
			: base(startsAt, body.Location, name)
		{
			_parameterList = parameterList;
			_body = body;
		}

		internal override AstNodeKind NodeKind { get { return AstNodeKind.Template; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				if(_parameterList != null)
					_parameterList.Accept(visitor);

				_body.Accept(visitor);
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("");
			writer.Write("function {0}", this.Name.Text);
			
			writer.Write(")");
			using(writer.CurlyBraces())
				writer.Write(_body);
		}

		public FunctionParameterList ParameterList { get { return _parameterList; } }
		public StatementBlock Body { get { return _body; } }

		
	}
}

