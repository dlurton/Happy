using System.Linq.Expressions;

namespace HappyTemplate.Compiler.Visitors
{
	interface IGlobalScopeHelper
	{
		Expression GetGlobalScopeGetter(string name);
		Expression GetGlobalScopeSetter(string name, Expression value);
	}
}