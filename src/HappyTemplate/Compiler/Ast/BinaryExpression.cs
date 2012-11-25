/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class BinaryExpression : ExpressionNodeBase
	{
		public readonly ExpressionNodeBase LeftValue;
		public readonly ExpressionNodeBase RightValue;
		public readonly Operator Operator;

		internal override AstNodeKind NodeKind { get { return AstNodeKind.BinaryExpression; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
			{
				this.LeftValue.Accept(visitor);
				visitor.AfterLeftExpressionVisit(this);
				this.RightValue.Accept(visitor);
				visitor.AfterRightExpressionVisit(this);
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("(");		
			using (writer.Indent())
			{

				writer.Write(this.LeftValue);
				writer.Write(this.Operator);
				writer.Write(this.RightValue);

			}
			writer.WriteLine(")");		
		}

		public BinaryExpression(ExpressionNodeBase lvalue, Operator oper, ExpressionNodeBase rvalue) 
			: base(lvalue.Location, rvalue.Location)
		{
			this.LeftValue = lvalue;
			this.RightValue = rvalue;
			this.Operator = oper;
		}
	}
}

