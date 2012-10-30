using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HappyTemplate.Compiler.Visitors;

namespace HappyTemplate.Compiler.Ast
{
	partial class UseStatement : StatementNodeBase
	{
		public string[] NamespaceSegments { get; private set; }

		public UseStatement(HappySourceLocation location, string[] namespaceSegments) : base(location)
		{
			this.NamespaceSegments = namespaceSegments;
		}

		public override AstNodeKind NodeKind
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
