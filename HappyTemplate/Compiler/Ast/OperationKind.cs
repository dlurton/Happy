namespace HappyTemplate.Compiler.Ast
{
	public enum OperationKind
	{
		Assign,
		Add,
		Subtract,
		Divide,
		Multiply,
		Mod,
		LogicalAnd,
		LogicalOr,
		Xor,
		Not,
		Equal,
		Greater,
		Less,
		GreaterThanOrEqual,
		LessThanOrEqual,
		NotEqual,
		MemberAccess,
		BitwiseOr,
		BitwiseAnd,
		Index
	}
}