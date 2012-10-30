using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class ContinueStatement : StatementNodeBase
	{
		public ContinueStatement(HappySourceLocation location) : base(location) {}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.ContinueStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("continue");
		}
	}
}
