namespace HappyTemplate.Compiler.Ast
{
	/// <summary>
	/// This class exists for no reason except to provide type safety.
	/// </summary>
	abstract class StatementNodeBase : AstNodeBase
	{
		protected StatementNodeBase(HappySourceLocation location) : base(location)
		{
			
		}
	}
}