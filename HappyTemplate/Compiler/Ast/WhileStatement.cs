using System;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class WhileStatement : LoopStatementBase
	{
		public ExpressionNodeBase Condition { get; private set; }
		public StatementBlock Block { get; private set; }
		public WhileStatement(HappySourceLocation location, ExpressionNodeBase condition, StatementBlock block) : base(location)
		{
			Block = block;
			this.Condition = condition;
		}

		public override AstNodeKind NodeKind { get { return AstNodeKind.WhileStatement; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			this.Condition.Accept(visitor);
			this.Block.Accept(visitor);
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
