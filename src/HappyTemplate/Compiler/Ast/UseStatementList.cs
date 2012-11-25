/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class UseStatementList : ScopeAstNodeBase
	{
		public UseStatement[] UseStatements { get; private set; }

		/// <summary>
		/// The use statement list doesn't really have a location
		/// TODO:  it probably shouldn't be a node type.  It should be just a property on Module
		/// </summary>
		/// <param name="useStatements"></param>
		public UseStatementList(UseStatement[] useStatements)
			: base(HappySourceLocation.None, HappySourceLocation.None)
		{
			this.UseStatements = useStatements;	
		}

		internal override AstNodeKind NodeKind
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

