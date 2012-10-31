using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	public class Module : ScopeAstNodeBase
	{
		internal DefStatement[] GlobalDefStatements { get; private set;  }

		internal UseStatementList UseStatements { get; private set; }
		internal LoadDirective[] LoadDirectives { get; private set; }

		internal Function[] Functions { get; private set; }

		internal Module(LoadDirective[] loadStmts, UseStatementList useStmts, DefStatement[] globalDefStmts, Function[] functions)
			: base(HappySourceLocation.None)
		{
			this.LoadDirectives = loadStmts;
			this.UseStatements = useStmts;
			this.GlobalDefStatements = globalDefStmts;
			this.Functions = functions;
		}

		public override AstNodeKind NodeKind
		{
			get { return AstNodeKind.Module; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.BeforeVisit(this);
			if (visitor.ShouldVisitChildren)
			{
				foreach (var loadStatement in this.LoadDirectives)
					loadStatement.Accept(visitor);

				this.UseStatements.Accept(visitor);

				foreach (var def in this.GlobalDefStatements)
					def.Accept(visitor);

				foreach (var func in this.Functions)
					func.Accept(visitor);
			}

			visitor.AfterVisit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.Write(this.LoadDirectives);
			writer.Write(this.UseStatements);
			writer.Write(this.GlobalDefStatements);
			writer.Write(this.Functions);
		}
	}
}