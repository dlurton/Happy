using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class NullExpression : ExpressionNodeBase
	{
		public override AstNodeKind NodeKind { get { return AstNodeKind.NullExpression; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("const null");
		}

		public NullExpression(HappySourceLocation loc) : base(loc)
		{
		}
	}
}