using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class UnaryExpression : ExpressionNodeBase
	{
		public ExpressionNodeBase Value { get; private set; }
		public Operator Operator { get; private set; }

		public UnaryExpression(ExpressionNodeBase value, Operator oper)
			: base(oper.Location)
		{
			this.Value = value;
			this.Operator = oper;
		}

		public override AstNodeKind NodeKind { get { return AstNodeKind.UnaryExpression; } }

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
