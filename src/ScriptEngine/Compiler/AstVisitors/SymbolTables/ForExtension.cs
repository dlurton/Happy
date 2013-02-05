namespace Happy.ScriptEngine.Compiler.AstVisitors.SymbolTables
{
	class ForExtension
	{
		HappyParameterSymbol _loopVariableSymbol;
		internal HappyParameterSymbol LoopVariableSymbol
		{
			get
			{
				return _loopVariableSymbol;
			}
			set
			{
				DebugAssert.IsNull(_loopVariableSymbol, "LoopVariableExpression may only be set once.");
				_loopVariableSymbol = value;
			}
		}
	}
}