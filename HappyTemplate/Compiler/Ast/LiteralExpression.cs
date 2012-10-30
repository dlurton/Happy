using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class LiteralExpression : ExpressionNodeBase
	{
		private readonly object _value;

		public object Value { get { return _value; } }
		public override AstNodeKind NodeKind { get { return AstNodeKind.LiteralExpression; } }
		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			if(_value is string)
				writer.WriteLine("\"{0}\"", _value);
			else
				writer.WriteLine("{1}, {0}", _value.GetType(), _value.ToString());
		}

		public LiteralExpression(HappySourceLocation span, object value)
			: base(span)
		{
			_value = value;
		}
	}
}