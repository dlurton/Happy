using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HappyTemplate;
using HappyTemplate.Runtime;
using Microsoft.Scripting;

namespace Happy
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
			if(args.Length == 0)
			{
				Console.Error.WriteLine("usage: <path to HappyTemplate script> [args passed script]...");
				Environment.ExitCode = -1;
				return;
			}

			try
			{
				runScript(args);
			}
			catch(Exception e)
			{
				Environment.ExitCode = -1;

				while (e != null)
				{
					Console.Error.WriteLine(e.Message);
					Console.Error.WriteLine(e.StackTrace);
					e = e.InnerException;
					if(e != null)
						Console.Error.Write("Inner ");
				}

			}
		}
		static HappyRuntimeContext compileModule(string module)
		{
			HappyRuntimeContext hrc = new HappyRuntimeContext(Console.Out);
			try
			{
				Action<HappyRuntimeContext> globalScopeInitializer = HappyCompiler.CompileModule(module);
				globalScopeInitializer(hrc);
			}
			catch (HappyTemplate.Exceptions.SourceException e)
			{
				throw new FatalException(string.Format("{0}: {1}", e.Location.Span.Start, e.Message));
			}
			catch (SyntaxErrorException se)
			{
				throw new FatalException(se.Message);
			}
			return hrc;
		}

		static string readSource(string moduleFile)
		{
			string module;
			try
			{
				module = readFile(moduleFile);
			}
			catch (Exception e)
			{
				throw new FatalException(string.Format("Couldn't open {0}\n{1}:  {2}", moduleFile, e.GetType(), e.Message));
			}
			return module;
		}

		static string readFile(string path)
		{
			using (var file = File.OpenText(path))
			{
				return file.ReadToEnd();
			}
		}
		static void runScript(string[] args)
		{
			string script = readSource(args[0]);
			HappyRuntimeContext runtimeContext = compileModule(script);
			runtimeContext.Globals._args = args.Skip(1).ToArray();
			dynamic mainFunction;

			var scope = (IDictionary<string, object>)runtimeContext.Globals;
			if(!scope.TryGetValue("main", out mainFunction))
				throw new FatalException("Function 'main' not present");
			mainFunction();
		}
	}
}
