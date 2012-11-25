/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Linq;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class FunctionCallExpression : NamedExpressionNodeBase
	{
		readonly ExpressionNodeBase[] _arguments;

		internal override AstNodeKind NodeKind { get { return AstNodeKind.FunctionCallExpression; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
				_arguments.ExecuteOverAll(node => node.Accept(visitor));
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("call {0} ", this.Identifier.Text);
			using(writer.Parens())
				_arguments.ExecuteOverAll(writer.Write);
		}

		public ExpressionNodeBase[] Arguments { get { return _arguments; } }

		public FunctionCallExpression(HappySourceLocation startsAt, HappySourceLocation endsAt, Identifier templateIdentifier, ExpressionNodeBase[] arguments)
			: base(startsAt, endsAt, templateIdentifier)
		{
			_arguments = arguments;
		}
	}
}

