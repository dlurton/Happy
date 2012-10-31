using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	class LoadDirective : AstNodeBase
	{
		public string AssemblyName { get; private set; }
		public LoadDirective(HappySourceLocation location, string assemblyName) : base(location)
		{
			this.AssemblyName = assemblyName;
		}

		public override AstNodeKind NodeKind
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
