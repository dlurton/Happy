using System.Linq.Expressions;

namespace HappyTemplate.Compiler.AstVisitors
{
	interface IGlobalScopeHelper
	{
		Expression GetGlobalScopeGetter(string name);
		Expression GetGlobalScopeSetter(string name, Expression value);
	}
}