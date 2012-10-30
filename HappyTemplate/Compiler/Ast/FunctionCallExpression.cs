using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class FunctionCallExpression : NamedExpressionNodeBase
	{
		readonly ExpressionNodeBase[] _arguments;

		public override AstNodeKind NodeKind { get { return AstNodeKind.FunctionCallExpression; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
				_arguments.ExecuteOverAll(node => node.Accept(visitor));
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("call {0} ", this.Identifier.Text);
			using(writer.Parens())
				_arguments.ExecuteOverAll(writer.Write);
		}

		public ExpressionNodeBase[] Arguments { get { return _arguments; } }

		public FunctionCallExpression(Identifier templateIdentifier, ExpressionNodeBase[] arguments)
			: base(templateIdentifier)
		{
			_arguments = arguments;
		}
	}
}