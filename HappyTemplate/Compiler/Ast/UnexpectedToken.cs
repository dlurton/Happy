using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	/// <summary>
	/// This is just a place-holder non-terminal used so
	/// that we can continue parsing after an unexpected token
	/// is enountered.
	/// </summary>
	/// <remarks>
	/// Normally, this would never be included as part of
	/// the AST unless we are also adding an error to the 
	/// ParserErrorCollector instance as well. 
	///  </remarks>
	internal class UnexpectedToken : AstNodeBase
	{
		public override AstNodeKind NodeKind { get { return AstNodeKind.UnexpectedToken; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("<unexpected token>");
		}

		public UnexpectedToken(HappySourceLocation span)
			: base(span)
		{

		}
	}
}