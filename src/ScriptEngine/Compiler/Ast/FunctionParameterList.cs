/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class FunctionParameterList : ScopeAstNodeBase
	{
		readonly FunctionParameter[] _parameters;
		public FunctionParameter this[int index] { get { return _parameters[index]; } }
		public FunctionParameterList(HappySourceLocation startsAt, HappySourceLocation endsAt, FunctionParameter[] parameters) : base(startsAt, endsAt)
		{
			_parameters = parameters;
		}

		public int Count { get { return _parameters.Length; }}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.FunctionParameterList; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				_parameters.ExecuteOverAll(p => p.Accept(visitor));

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write("(");
			writer.Write(_parameters, ", ");
			writer.Write(")");
		}

		
	}
}

