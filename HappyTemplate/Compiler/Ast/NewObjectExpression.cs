using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class NewObjectExpression : ExpressionNodeBase
	{
		public ExpressionNodeBase TypeExpression { get; private set; }
		public ExpressionNodeBase[] ConstructorAgs { get; private set; }

		public NewObjectExpression(ExpressionNodeBase typeExpression, ExpressionNodeBase[] constructorArgs) : base(typeExpression.Location)
		{
			this.TypeExpression = typeExpression;
			this.ConstructorAgs = constructorArgs;
		}

		public override AstNodeKind NodeKind
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
