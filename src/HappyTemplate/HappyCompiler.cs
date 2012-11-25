/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

#if DEBUG
//#define WRITE_AST
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HappyTemplate.Compiler;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.AstVisitors;
using HappyTemplate.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace HappyTemplate
{
	public class HappyCompiler
	{
		public static Action<HappyRuntimeContext> CompileModule(string filename)
		{
			ScriptRuntime runtime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration());
			ScriptEngine engine = runtime.GetEngine("ht");
			ScriptScope globals = engine.CreateScope();
			var scriptSource = engine.CreateScriptSourceFromString(readFile(filename), filename, SourceCodeKind.File);
			return scriptSource.Execute(globals);
		}

		static string readFile(string path)
		{
			using (var file = File.OpenText(path))
			{
				return file.ReadToEnd();
			}
		}
	}
}

