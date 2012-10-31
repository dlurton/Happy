using System;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class OutputStatement : StatementNodeBase
	{
		readonly ExpressionNodeBase[] _expressionsToWrite;

		public OutputStatement(HappySourceLocation location, ExpressionNodeBase[] expressionsToWrite)
			: base(location) { _expressionsToWrite = expressionsToWrite; }


		public ExpressionNodeBase[] ExpressionsToWrite { get { return _expressionsToWrite; } }
		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.OutputWrite;  }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
			{
				foreach (ExpressionNodeBase exp in _expressionsToWrite)
					exp.Accept(visitor);
			}
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("~");
			using(writer.CurlyBraces())
				_expressionsToWrite.ExecuteOverAll(writer.Write);
		}
	}
}