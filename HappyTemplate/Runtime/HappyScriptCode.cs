using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace HappyTemplate.Runtime
{
	public class HappyScriptCode : ScriptCode
	{
		readonly Delegate _compiledLambda;

		public HappyScriptCode(SourceUnit sourceUnit, Delegate compiledLambda)
			: base(sourceUnit)
		{
			_compiledLambda = compiledLambda;
		}

		public override object Run(Scope scope)
		{
			return _compiledLambda.DynamicInvoke(scope.Storage); 
		}
	}
}