using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	class DefStatement : StatementNodeBase
	{
		public VariableDef[] VariableDefs { get; private set; }
		public DefStatement(HappySourceLocation location, VariableDef[] variableDefs) : base(location)
		{
			this.VariableDefs = variableDefs;
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.DefStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);

			if (visitor.ShouldVisitChildren)
				this.VariableDefs.ExecuteOverAll(vd => vd.Accept(visitor));

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			
			writer.Write("def ");
			using(writer.Indent())
				writer.Write(this.VariableDefs);
		}
	}
}
