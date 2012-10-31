using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	internal class VerbatimSection : StatementNodeBase
	{
		readonly string _text;

		public override AstNodeKind NodeKind { get { return AstNodeKind.VerbatimSection; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("verbatim length: {0}, startsAt: ({1})>", _text.Length, this.Location);
		}

		public string Text { get { return _text; } }

		public VerbatimSection(HappySourceLocation span, string output)
			: base(span)
		{
			_text = output;
		}
	}
}