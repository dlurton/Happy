/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class VariableDef : NamedAstNodeBase
	{
		public ExpressionNodeBase InitializerExpression { get; private set; }

		public VariableDef(Identifier identifier, ExpressionNodeBase defaultExpression) 
			: base(identifier.Location, defaultExpression != null ? defaultExpression.Location : null, identifier)
		{
			this.InitializerExpression = defaultExpression;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.VariableDef; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren && this.InitializerExpression != null)
				this.InitializerExpression.Accept(visitor);

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			if(this.InitializerExpression == null)
				writer.Write(this.Name.Text);
			else
			{
				writer.Write(this.Name.Text);
				writer.Write(" = ");
				writer.Write(this.InitializerExpression);
			}
		}
	}
}

