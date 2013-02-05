/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Happy.ScriptEngine;
using Happy.ScriptEngine.Exceptions;
using Happy.ScriptEngine.Runtime;
using Microsoft.Scripting;

namespace Happy.CommandLine
{
	class Program
	{
		class FatalException : Exception
		{
			public FatalException(string message)
				: base(message)
			{

			}
		}

		static void Main(string[] args)
		{
			if(args.Length < 2)
			{
				Console.Error.WriteLine("usage: <path to HappyTemplate script> <path to output file> [args passed script]...");
				Environment.ExitCode = -1;
				return;
			}

			/*try
			{*/
				runScript(args);
			/*}
			catch(Exception e)
			{
				Environment.ExitCode = -1;

				while (e != null)
				{
					Console.Error.WriteLine(e.GetType().FullName + ": " + e.Message);
					Console.Error.WriteLine(e.StackTrace);
					e = e.InnerException;
					if(e != null)
						Console.Error.Write("Inner ");
				}

			}*/
		}
		static HappyRuntimeContext compileModule(string filename)
		{
			HappyRuntimeContext hrc = new HappyRuntimeContext(Console.Out);
			try
			{
				Action<HappyRuntimeContext> runtimeContextInitializer = HappyCompiler.CompileModule(filename);
				runtimeContextInitializer(hrc);
			}
			catch (SourceException e)
			{
				throw new FatalException(string.Format("{0}: {1}", e.Location.Span.Start, e.Message));
			}
			catch (SyntaxErrorException se)
			{
				throw new FatalException(se.Message);
			}
			return hrc;
		}
		
		static void runScript(string[] args)
		{
			HappyRuntimeContext runtimeContext = compileModule(args[0]);

            using (var stream = File.Open(args[1], FileMode.Create))
            using(var writer = new StreamWriter(stream))
            {
                dynamic mainFunction;
                var scope = (IDictionary<string, object>) runtimeContext.Globals;
                if (!scope.TryGetValue("main", out mainFunction))
                    throw new FatalException("Function 'main' not present");

                runtimeContext.PushWriter(writer);
                mainFunction(args.Skip(2).ToArray());
                runtimeContext.PopWriter();
            }
		}
	}
}

