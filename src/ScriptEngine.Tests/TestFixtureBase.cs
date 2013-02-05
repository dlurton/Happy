/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.IO;
using System.Text;
using Happy.ScriptEngine.Compiler;
using Happy.ScriptEngine.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using NUnit.Framework;

namespace Happy.ScriptEngine.Tests
{
	public class TestFixtureBase
	{
		protected ScriptRuntime Runtime;
		protected Microsoft.Scripting.Hosting.ScriptEngine Engine;
		protected ScriptScope GlobalScope;
		private MemoryStream _output;
		private MemoryStream _errorOutput;
		protected HappyLanguageContext LanguageContext;

		[SetUp]
		public void SetUp()
		{
			var configFile = Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(TestFixtureBase).Assembly.CodeBase).AbsolutePath)) + ".config";
			Runtime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));

			_output = new MemoryStream();
			_errorOutput = new MemoryStream();

			Runtime.IO.SetOutput(_output, Encoding.ASCII);
			Runtime.IO.SetErrorOutput(_errorOutput, Encoding.ASCII);

			Engine = Runtime.GetEngine("ht");

			LanguageContext = HostingHelpers.GetLanguageContext(Engine) as HappyLanguageContext;

			GlobalScope = Engine.CreateScope();

			Assert.IsNotNull(GlobalScope);
		}

		protected HappyRuntimeContext CompileModule(string script)
		{
			HappyRuntimeContext retval = new HappyRuntimeContext(new StringWriter());
			var ss = Engine.CreateScriptSourceFromString(script, "<unit test>", SourceCodeKind.File);
			dynamic runtimeContextInitializer = ss.Execute();
			
			runtimeContextInitializer(retval);
			return retval;
		}

		protected HappyRuntimeContext CompileFunction(string expression, string globals = null)
		{
			return CompileFunction("", expression, globals);
		}

		protected HappyRuntimeContext CompileFunction(string parameters, string expression, string globals = null)
		{
			return this.CompileModule((globals != null ? "def " + globals + ";\n" : "") + @"function testFunc(" + parameters + ") \n{\n" + expression + "\n}\n");
		}

		protected HappyRuntimeContext CompileExpression(string expression)
		{
			return this.CompileModule(@"function testFunc() { def e; e = " + expression + @"return e; }");
		}

		

		protected internal static void NextTokenAssert(Lexer lexer, HappyTokenKind expectedHappyTokenKind)
		{
			Token t = lexer.NextToken();
			Assert.AreEqual(expectedHappyTokenKind, t.HappyTokenKind, "Token Text:  {0}", t.Text);
		}

		protected internal static Token NextTokenAssert(Lexer lexer, HappyTokenKind expectedHappyTokenKind, string expectedText)
		{
			Token t = lexer.NextToken();
			Assert.AreEqual(expectedHappyTokenKind, t.HappyTokenKind, "Token Text:  {0}", t.Text);
			Assert.AreEqual(expectedText, t.Text);
			return t;
		}

		protected internal void AssertNoErrors()
		{
			Assert.AreEqual(0, LanguageContext.ErrorSink.ErrorCount, "Wanted zero errors but {0} were found.", LanguageContext.ErrorSink.ErrorCount);
		}

		protected string Execute(string snippet, string globals = null)
		{
			return Execute(snippet, null, globals);
		}

		protected string Execute(string snippet, Action<dynamic> globalInitializer, string globals = null)
		{
			var rc = CompileStatement(snippet, globals);
			if(globalInitializer != null)
				globalInitializer(rc.Globals);
			rc.Globals.testFunc();
			return rc.OutputWriter.ToString();
		}

		HappyRuntimeContext CompileStatement(string snippet, string globals = null)
		{
			string module = (globals != null ? "def " + globals + "; " : "") + "\nfunction testFunc() {\n" + snippet + "\n}";
			HappyRuntimeContext rc = this.CompileModule(module);
			return rc;
		}


		protected void AssertOutput(string expectedOutput, string expression, string globals = null)
		{
			Assert.AreEqual(expectedOutput, Execute(expression, null, null));
		}

		protected void AssertOutput(string expectedOutput, string expression, Action<dynamic> globalInitializer, string globals = null)
		{	
			Assert.AreEqual(expectedOutput, Execute(expression, globalInitializer, globals));
		}
		#region Test Support Methods

		internal void AssertCompileErrorInModule(ErrorCode code, string module)
		{
			AssertSyntaxErrorException(code, null, () => this.CompileModule(module));
		}

		internal void AssertCompileErrorInModule(ErrorCode code, string expectedMessage, string module)
		{
			AssertSyntaxErrorException(code, expectedMessage, () => this.CompileModule(module));
		}

		internal void AssertSyntaxErrorInStatement(ErrorCode code, string expectedMessage, string statement)
		{
			AssertSyntaxErrorException(code, expectedMessage, () => this.CompileStatement(statement));
		}

		internal void AssertSyntaxErrorInStatement(ErrorCode code, string statement)
		{
			AssertSyntaxErrorException(code, () => this.CompileStatement(statement));
		}

		internal static void AssertSyntaxErrorException(ErrorCode errorCode, Action action)
		{
			AssertSyntaxErrorException(errorCode, null, action);
		}

		internal static void AssertSyntaxErrorException(ErrorCode errorCode, string expectedMessage, Action action)
		{
			try
			{
				action();
				Assert.Fail("SyntaxErrorException not thrown as expected\nThrown");
			}
			catch (SyntaxErrorException e)
			{
				//Console.WriteLine(e.StackTrace);
				Assert.AreEqual(errorCode, (ErrorCode)e.ErrorCode, "Unexpected syntax error code.  The message was:  " + e.Message);
				if(!String.IsNullOrEmpty(expectedMessage))
					Assert.AreEqual(expectedMessage, expectedMessage, "The error code matched but the message was different than what was expected.");
			}
		}
		#endregion
	}
}

