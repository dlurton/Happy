/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Linq;
using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class DefStatement : StatementNodeBase
	{
		public VariableDef[] VariableDefs { get; private set; }
		public DefStatement(HappySourceLocation startsAt, VariableDef[] variableDefs) : base(startsAt, variableDefs.Last().Location)
		{
			this.VariableDefs = variableDefs;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.DefStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				this.VariableDefs.ExecuteOverAll(vd => vd.Accept(visitor));

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			
			writer.Write("def ");
			using(writer.Indent())
				writer.Write(this.VariableDefs);
		}
	}
}

