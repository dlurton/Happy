/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Linq;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class OutputStatement : StatementNodeBase
	{
		readonly ExpressionNodeBase[] _expressionsToWrite;

		public OutputStatement(ExpressionNodeBase[] expressionsToWrite)
			: base(expressionsToWrite.First().Location, expressionsToWrite.Last().Location) { _expressionsToWrite = expressionsToWrite; }


		public ExpressionNodeBase[] ExpressionsToWrite { get { return _expressionsToWrite; } }
		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.OutputWrite;  }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
			{
				foreach (ExpressionNodeBase exp in _expressionsToWrite)
					exp.Accept(visitor);
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("~");
			using(writer.CurlyBraces())
				_expressionsToWrite.ExecuteOverAll(writer.Write);
		}
	}
}

