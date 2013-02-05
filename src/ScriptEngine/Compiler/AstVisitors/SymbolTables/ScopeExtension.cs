namespace Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables
{
	class ScopeExtension
	{
		HappySymbolTable _symbolTable;
		internal HappySymbolTable SymbolTable
		{
			get
			{
				return _symbolTable;
			}
			set
			{
				DebugAssert.IsNull(_symbolTable, "SymbolTable may only be set once.");
				_symbolTable = value;
			}
		}
	}
}