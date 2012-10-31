using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class VariableDef : NamedAstNodeBase
	{
		public ExpressionNodeBase InitializerExpression { get; private set; }

		public VariableDef(Identifier identifier, ExpressionNodeBase defaultExpression) : base(identifier)
		{
			this.InitializerExpression = defaultExpression;
		}

		public override AstNodeKind NodeKind
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
