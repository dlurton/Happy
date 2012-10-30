using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	internal class IdentifierExpression : NamedExpressionNodeBase
	{
		public override AstNodeKind NodeKind { get { return AstNodeKind.IdentifierExpression; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("[{0}]", this.Identifier.Text);
		}

		public IdentifierExpression(Identifier identifier) : base(identifier)
		{
			
		}
	}
}