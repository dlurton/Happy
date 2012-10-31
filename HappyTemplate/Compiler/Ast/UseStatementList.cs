using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class UseStatementList : ScopeAstNodeBase
	{
		public UseStatement[] UseStatements { get; private set; }

		public UseStatementList(UseStatement[] useStatements) : base(HappySourceLocation.None)
		{
			this.UseStatements = useStatements;	
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.UseStatementList; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
				this.UseStatements.ExecuteOverAll(u => u.Accept(visitor));

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write(this.UseStatements);
		}
	}
}
