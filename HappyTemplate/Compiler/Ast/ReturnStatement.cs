using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class ReturnStatement : StatementNodeBase
	{
		public ExpressionNodeBase ReturnExp { get; private set; }
		public ReturnStatement(HappySourceLocation location, ExpressionNodeBase returnExp)
			: base(location)
		{
			this.ReturnExp = returnExp;
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.ReturnStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (!visitor.ShouldVisitChildren)
				return;

			if(this.ReturnExp != null)
				this.ReturnExp.Accept(visitor);
			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("return ");
			if(this.ReturnExp != null)
				using(writer.Indent())
					writer.Write(this.ReturnExp);
		}
	}
}
