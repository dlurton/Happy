using HappyTemplate.Compiler.AstVisitors;

namespace HappyTemplate.Compiler.Ast
{
	//TO DO:  need to eliminate this as a public class
	//to do that, first have to remove Ast.Template from the 
	//public API.
	public abstract class AstNodeBase
	{
		static int _numAstNodes;
		readonly HappySourceLocation _location;

		public HappySourceLocation Location { get { return _location; } }
		public abstract AstNodeKind NodeKind { get; }
		public int Id { get; private set; }

		protected AstNodeBase(HappySourceLocation location)
		{
			_location = location;
			this.Id = ++_numAstNodes;
		}

		internal abstract void Accept(AstVisitorBase visitor);
	    internal abstract void WriteString(AstWriter writer);
	}
}