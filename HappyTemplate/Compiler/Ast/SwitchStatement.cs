﻿using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class SwitchStatement : StatementNodeBase
	{
		public SwitchCase[] Cases { get; private set; }
		public ExpressionNodeBase SwitchExpression { get; private set; }
		public StatementBlock DefaultStatementBlock { get; private set; }

		public SwitchStatement(ExpressionNodeBase switchExpression, SwitchCase[] cases, StatementBlock defaultCase) : base(switchExpression.Location)
		{
			this.SwitchExpression = switchExpression;
			this.Cases = cases;
			this.DefaultStatementBlock = defaultCase;
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.SwitchStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				this.SwitchExpression.Accept(visitor);
				this.Cases.ExecuteOverAll(c => c.Accept(visitor));
				if (this.DefaultStatementBlock != null)
					this.DefaultStatementBlock.Accept(visitor);
			}

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("switch");
			using(writer.CurlyBraces())
			{
				this.SwitchExpression.WriteString(writer);
				this.Cases.ForAllBetween(c => c.WriteString(writer), () => writer.Write("\n"));
				if(this.DefaultStatementBlock != null)
				{
					writer.WriteLine("default:");
					this.DefaultStatementBlock.WriteString(writer);
				}
			}

		}
	}
}
