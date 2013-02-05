/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class NewObjectExpression : ExpressionNodeBase
	{
		public ExpressionNodeBase TypeExpression { get; private set; }
		public ExpressionNodeBase[] ConstructorAgs { get; private set; }

		public NewObjectExpression(HappySourceLocation startsAt, HappySourceLocation endsAt, ExpressionNodeBase typeExpression, ExpressionNodeBase[] constructorArgs) 
			: base(startsAt, endsAt)
		{
			this.TypeExpression = typeExpression;
			this.ConstructorAgs = constructorArgs;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.NewObjectExpression;  }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{

				this.TypeExpression.Accept(visitor);
				this.ConstructorAgs.ExecuteOverAll(arg => arg.Accept(visitor));
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write("new ");
			using(writer.Parens())
			{
				writer.Write(this.TypeExpression);
				writer.Write(this.ConstructorAgs);
			}
		}
	}
}

