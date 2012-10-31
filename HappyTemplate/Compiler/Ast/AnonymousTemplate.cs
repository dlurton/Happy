using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class AnonymousTemplate : ExpressionNodeBase
	{
		public StatementBlock Body { get; private set; }

		public override AstNodeKind NodeKind { get { return AstNodeKind.AnonymousTemplate; } }

		public AnonymousTemplate(StatementBlock body)
			: base(body.Location)
		{
			this.Body = body;
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				this.Body.Accept(visitor);

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			using(writer.Block("<|", "|>"))
				writer.Write(this.Body);
		}
	}
}