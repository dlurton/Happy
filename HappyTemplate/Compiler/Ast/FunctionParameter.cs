using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	internal class FunctionParameter : NamedAstNodeBase
	{
		public override AstNodeKind NodeKind { get { return AstNodeKind.TemplateParameter; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write(this.Name.Text);
		}

		public FunctionParameter(Identifier name)
			: base(name)
		{
		}
	}
}