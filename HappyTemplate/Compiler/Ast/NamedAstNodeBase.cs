using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyTemplate.Compiler.Ast
{
	abstract partial class NamedAstNodeBase : AstNodeBase
	{
		public Identifier Name { get; private set; }

		protected NamedAstNodeBase(Identifier name) : base(name.Location)
		{
			this.Name = name;
		}
	}
}
