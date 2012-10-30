namespace HappyTemplate.Compiler.Ast
{
	abstract class NamedExpressionNodeBase : ExpressionNodeBase
	{
		readonly protected Identifier _identifier;

		public Identifier Identifier { get { return _identifier; } }

		protected NamedExpressionNodeBase(Identifier identifier)
			: base(identifier.Location)
		{
			_identifier = identifier;
		}

		public override string ToString()
		{
			return this.Identifier.ToString();
		}
	}
}