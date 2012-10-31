using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class SwitchCase : AstNodeBase
	{
		public ExpressionNodeBase[] CaseValues{ get; private set; }
		public StatementBlock CaseStatementBlock { get; private set; }

		public SwitchCase(ExpressionNodeBase[] caseValues, StatementBlock statements) : base(caseValues[0].Location)
		{
			this.CaseValues = caseValues;
			this.CaseStatementBlock = statements;
		}

		public override AstNodeKind NodeKind
		{
			get
			{
				return AstNodeKind.SwitchCase;
			}
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				this.CaseValues.ForAll(cv => cv.Accept(visitor));
				this.CaseStatementBlock.Accept(visitor);
			}

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("case ");
			using(writer.CurlyBraces())
			{
				this.CaseValues.ForAllBetween(cv => cv.WriteString(writer), () => writer.Write(", "));
				writer.WriteLine(":");
				this.CaseStatementBlock.WriteString(writer);
			}
		}
	}
}
