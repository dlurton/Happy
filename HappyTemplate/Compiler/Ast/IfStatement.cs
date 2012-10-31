using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class IfStatement : StatementNodeBase
	{
		private readonly ExpressionNodeBase _condition;
		private readonly StatementBlock _trueStatementBlock;
		private readonly StatementBlock _falseStatementBlock;

		public override AstNodeKind NodeKind { get { return AstNodeKind.IfStatement; } }

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				_condition.Accept(visitor);
				visitor.AfterIfStatementCondition(this);
				_trueStatementBlock.Accept(visitor);
				visitor.AfterIfStatementTrueBlock(this);
				if (_falseStatementBlock != null)
				{
					_falseStatementBlock.Accept(visitor);
					visitor.AfterIfStatementFalseBlock(this);
				}
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("if");
			using(writer.Parens())
				writer.Write(_condition);
			writer.WriteLine("then");
			
			using(writer.Indent())
				writer.Write(_trueStatementBlock);

			if (_falseStatementBlock != null)
			{
				writer.WriteLine("else");
				using (writer.CurlyBraces())
					writer.Write(_falseStatementBlock);
			}
			
		}

		public ExpressionNodeBase Condition { get { return _condition; } }
		public StatementBlock TrueStatementBlock { get { return _trueStatementBlock; } }
		public StatementBlock FalseStatementBlock { get { return _falseStatementBlock; } }

		public IfStatement(ExpressionNodeBase  value, StatementBlock trueStatementBlock, StatementBlock falseStatementBlock)
			: base(value.Location)
		{
		
			_condition = value;
			_trueStatementBlock = trueStatementBlock;
			_falseStatementBlock = falseStatementBlock;
		}
	}
}