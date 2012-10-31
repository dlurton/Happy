#if DEBUG
//#define WRITE_AST
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HappyTemplate.Compiler;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.AstVisitors;
using HappyTemplate.Runtime;
using Microsoft.Scripting.Hosting;

namespace HappyTemplate
{
	public class HappyCompiler
	{
		public static Action<HappyRuntimeContext> CompileModule(string template)
		{
			ScriptRuntime runtime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration());
			ScriptEngine engine = runtime.GetEngine("ht");
			ScriptScope globals = engine.CreateScope();
			return engine.Execute(template, globals);
		}
	}
}
