using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HappyTemplate;
using HappyTemplate.Compiler;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace HappyTemplate.TestsGenerator
{
	class ExpressionTest
	{
		public string Expression { get; private set; }
		public string Expected { get; private set; }
		public bool ExpectException { get; private set; }
		public Type ExpectedType { get; private set; }

		public ExpressionTest(string expression, string expected, bool expectException, Type expectedType)
		{
			this.Expression = expression;
			this.Expected = expected;
			this.ExpectException = expectException;
			this.ExpectedType = expectedType;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("HappyTemplate.TestsGenerator.ExpressionTests.hts"));

				var configFile = Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(Program).Assembly.CodeBase).AbsolutePath)) + ".config";
				ScriptRuntime scriptRuntime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));
				ScriptEngine scriptEngine = scriptRuntime.GetEngine(@"ht");

				ScriptScope globals = scriptEngine.CreateScope();
				scriptEngine.Execute(sr.ReadToEnd(), globals);

				using (FileStream output = File.Open(@"..\..\..\HappyTemplate.Tests\GeneratedTests.cs", FileMode.Create))
				using (StreamWriter writer = new StreamWriter(output))
				{
					HappyRuntimeContext rc = new HappyRuntimeContext(scriptEngine,  writer, globals);

					rc.Globals.tests = LoadTests();

					rc.Globals.main();
				}
			}
			catch (Exception e)
			{
				HandleException(e, "While parsing embedded template set:");
			}
		}

		private static IEnumerable<ExpressionTest> LoadTests()
		{
			using(FileStream fs = File.OpenRead("ExpressionTests.dat"))
			using(StreamReader reader = new StreamReader(fs))
			{
				List<ExpressionTest> tests = new List<ExpressionTest>();
				int lineNo = 1;
				while(!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					string[] splitted = line.Split(':');
					string expectedTypeName;
					bool expectException = false;
					string expectedValue = "<ERROR>";
					if(splitted.Length == 2)
					{
						expectException = true;
						expectedTypeName = splitted[1];
					} else if(splitted.Length == 3)
					{
						expectedValue = splitted[1];
						expectedTypeName = splitted[2];
					}
					else
					{
						Console.WriteLine("Expression test on line {0} did not have 2 or 3 segments.", lineNo);
						continue;
					}


					Type expectedType = Type.GetType(expectedTypeName);
					if(expectedType == null)
					{
						Console.WriteLine("Expression test on line {0} did not have a loadable expected type.", lineNo);
						continue;
					}



					tests.Add(new ExpressionTest(splitted[0], expectedValue, expectException, expectedType));

					lineNo++;
				}
				return tests;
			}
		}

		static void HandleException(Exception e, string whileFmt, params object[] args)
		{
			if (!String.IsNullOrEmpty(whileFmt))
			{
				Console.Error.WriteLine(whileFmt, args);
			}

			Console.Error.WriteLine("Exception:  {0}\nMessage:  {1}", e.GetType(), e.Message);
		}

		//static void Main()
		//{

			//try
			//{
			//    StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("OmniUnitTestGenerator.UnitTests.hts"));

			//    var configFile = Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(Program).Assembly.CodeBase).AbsolutePath)) + ".config";
			//    ScriptRuntime scriptRuntime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));
			//    ScriptEngine scriptEngine = scriptRuntime.GetEngine(@"ht");

			//    ScriptScope globals = scriptEngine.CreateScope();
			//    scriptEngine.Execute(sr.ReadToEnd(), globals);

			//    using(FileStream output = File.Open(@"..\..\..\HappyTemplate.Tests\GeneratedTests.cs", FileMode.Create))
			//    using(StreamWriter writer = new StreamWriter(output))
			//    {
			//        HappyRuntimeContext rc = new HappyRuntimeContext(writer, globals);

			//        //TO DO:  replace the below line with a literal array
			//        rc.Globals.comparableTypes = new[]
			//        {
			//            typeof(Byte),
			//            typeof(SByte),
			//            //typeof(Char),
			//            typeof(UInt16),
			//            typeof(Int16),
			//            typeof(UInt32),
			//            typeof(Int32),
			//            typeof(Single),
			//            typeof(Double),
			//            typeof(Decimal)
			//        };

			//        rc.Globals.main();
			//    }
			//}
			//catch (Exception e)
			//{
			//    HandleException(e, "While parsing embedded template set:");
			//}

			
		//}
	}
}
