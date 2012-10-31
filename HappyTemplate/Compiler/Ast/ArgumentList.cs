using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class ArgumentList : ExpressionNodeBase 
	{
		public ExpressionNodeBase[] Arguments { get; private set; }
		public ArgumentList(HappySourceLocation location, ExpressionNodeBase[] arguments)
			: base(location) 
		{
			this.Arguments = arguments;
		}

		public override AstNodeKind NodeKind  { get { return AstNodeKind.ArgumentList; } 
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren) 
				foreach (ExpressionNodeBase arg in this.Arguments)
					arg.Accept(visitor);

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			using(writer.Parens())
				writer.Write(this.Arguments);
		}
	}
}
