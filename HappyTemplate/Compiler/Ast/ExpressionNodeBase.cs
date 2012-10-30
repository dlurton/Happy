namespace HappyTemplate.Compiler.Ast
{
	/// <summary>
	/// This class doesn't do anything except provide some additional type-saftey when working with the AST.
	/// </summary>
	abstract class ExpressionNodeBase : StatementNodeBase
	{
		protected ExpressionNodeBase(HappySourceLocation location)
			: base(location)
		{
			this.AccessType = ExpressionAccessType.Read;
		}

		public ExpressionAccessType AccessType { get; set; }
	}
}
