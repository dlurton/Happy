/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class UseStatement : StatementNodeBase
	{
		public string[] NamespaceSegments { get; private set; }

		public UseStatement(HappySourceLocation startsAt, HappySourceLocation endsAt, string[] namespaceSegments) : base(startsAt, endsAt)
		{
			this.NamespaceSegments = namespaceSegments;
		}

		internal override AstNodeKind NodeKind
		{
			get { return AstNodeKind.UseStatement; }
		}

		internal override void Accept(AstVisitorBase visitor)
		{
			visitor.Visit(this);
		}

		internal override void WriteString(AstWriter writer)
		{
			writer.WriteLine("use {0}", String.Join(".", this.NamespaceSegments));
		}
	}
}

