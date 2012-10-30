using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class BreakStatement : StatementNodeBase
	{
		public BreakStatement(HappySourceLocation location) : base(location)
		{
			
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.BreakStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("break");
		}
	}
}
