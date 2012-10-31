using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	partial class StatementBlock : ScopeAstNodeBase
	{
		public AstNodeBase[] Statements { get; private set; }
		public StatementBlock(HappySourceLocation location, AstNodeBase[] stmts)
			: base(location)
		{
			this.Statements = stmts;
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.BlockStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				this.Statements.ExecuteOverAll(n => n.Accept(visitor));
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			using(writer.CurlyBraces())
				this.Statements.ExecuteOverAll(writer.Write);
		}

		public AstNodeBase this[int i]
		{
			get
			{
				return this.Statements[i];
			}
		}

	}
}
