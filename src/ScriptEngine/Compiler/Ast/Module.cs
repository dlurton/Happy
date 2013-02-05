/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;
using Microsoft.Scripting.Utils;

namespace Happy.ScriptEngine.Compiler.Ast
{
	public class Module : ScopeAstNodeBase
	{
		internal DefStatement[] GlobalDefStatements { get; private set;  }

		internal UseStatementList UseStatements { get; private set; }
		internal LoadDirective[] LoadDirectives { get; private set; }

		internal Function[] Functions { get; private set; }

		internal Module(LoadDirective[] loadStmts, UseStatementList useStmts, DefStatement[] globalDefStmts, Function[] functions)
			: base(HappySourceLocation.None, HappySourceLocation.None)
		{
			ContractUtils.RequiresNotNull(loadStmts, "loadStmts");
			ContractUtils.RequiresNotNull(useStmts, "useStmts");
			ContractUtils.RequiresNotNull(globalDefStmts, "globalDefStmts");
			ContractUtils.RequiresNotNull(functions,  "functions");

			this.LoadDirectives = loadStmts;
			this.UseStatements = useStmts;
			this.GlobalDefStatements = globalDefStmts;
			this.Functions = functions;
		}

		internal override AstNodeKind NodeKind
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

				if(this.UseStatements != null)
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

