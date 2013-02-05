/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler.AstVisitors;

namespace Happy.ScriptEngine.Compiler.Ast
{
	class LoadDirective : AstNodeBase
	{
		public string AssemblyName { get; private set; }
		public LoadDirective(HappySourceLocation startsAt, HappySourceLocation endsAt, string assemblyName) : base(startsAt, endsAt)
		{
			this.AssemblyName = assemblyName;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.LoadStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("load \"{0}\"", this.AssemblyName);
		}
	}
}

