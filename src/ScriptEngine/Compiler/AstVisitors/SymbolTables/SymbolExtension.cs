namespace Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables
{
	class SymbolExtension
	{
		HappySymbolBase _symbol;
		internal HappySymbolBase Symbol
		{
			get
			{
				return _symbol;
			}
			set
			{
				DebugAssert.IsNull(_symbol, "Symbol may only be set once.");
				_symbol = value;
			}
		}
	}
}