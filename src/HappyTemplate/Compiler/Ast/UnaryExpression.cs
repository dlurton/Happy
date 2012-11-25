/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class UnaryExpression : ExpressionNodeBase
	{
		public ExpressionNodeBase Value { get; private set; }
		public Operator Operator { get; private set; }

		public UnaryExpression(ExpressionNodeBase value, Operator oper)
            : base(oper.Location, value.Location)
		{
			this.Value = value;
			this.Operator = oper;
		}

		internal override AstNodeKind NodeKind { get { return AstNodeKind.UnaryExpression; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
				this.Value.Accept(visitor);

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write(this.Operator);
			using(writer.Parens())
				writer.Write(this.Value);
		}
	}
}

