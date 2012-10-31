using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class BinaryExpression : ExpressionNodeBase
	{
		public readonly ExpressionNodeBase LeftValue;
		public readonly ExpressionNodeBase RightValue;
		public readonly Operator Operator;

		public override AstNodeKind NodeKind { get { return AstNodeKind.BinaryExpression; } }
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

		public BinaryExpression(ExpressionNodeBase lvalue, Operator oper, ExpressionNodeBase rvalue) : base(oper.Location)
		{
			this.LeftValue = lvalue;
			this.RightValue = rvalue;
			this.Operator = oper;
		}
	}
}