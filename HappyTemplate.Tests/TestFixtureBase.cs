using System;
using System.Dynamic;
using System.IO;
using System.Text;
using HappyTemplate.Compiler;
using HappyTemplate.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using NUnit.Framework;

namespace HappyTemplate.Tests
{
	public class TestFixtureBase
	{
		protected ScriptRuntime _runtime;
		protected ScriptEngine _engine;
		protected ScriptScope _globalScope;
		private MemoryStream _output;
		private MemoryStream _errorOutput;
		protected HappyLanguageContext _languageContext;
		[SetUp]
		public void SetUp()
		{
			var configFile = Path.GetFullPath(Uri.UnescapeDataString(new Uri(typeof(TestFixtureBase).Assembly.CodeBase).AbsolutePath)) + ".config";
			_runtime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));

			_output = new MemoryStream();
			_errorOutput = new MemoryStream();

			_runtime.IO.SetOutput(_output, Encoding.ASCII);
			_runtime.IO.SetErrorOutput(_errorOutput, Encoding.ASCII);

			_engine = _runtime.GetEngine("ht");

			_languageContext = HostingHelpers.GetLanguageContext(_engine) as HappyLanguageContext;

			_globalScope = _engine.CreateScope();

			Assert.IsNotNull(_globalScope);

		}

		internal Lexer TestLexer(string text)
		{
			return new Lexer(_languageContext.CreateSnippet(text, SourceCodeKind.AutoDetect), _languageContext);
		}

		internal Lexer TestVerbatimLexer(string text)
		{
			Lexer retval = new Lexer(_languageContext.CreateSnippet("<|" + text + "|>", SourceCodeKind.AutoDetect), _languageContext);
			NextTokenAssert(retval, HappyTokenKind.BeginTemplate);
			Assert.AreEqual(Lexer.LexerMode.LexingVerbatimText, retval.Mode);
			return retval;
		}

		protected HappyRuntimeContext CompileModule(string script)
		{
			//ScriptScope globals = _engine.CreateScope();
			dynamic globals = new ExpandoObject();
			HappyRuntimeContext retval = new HappyRuntimeContext(new StringWriter());
			dynamic globalScopeInitializer = _engine.Execute(script);
			globalScopeInitializer(retval);

			return retval;
		}
		protected HappyRuntimeContext CompileFunction(string expression, string globals = null)
		{
			return CompileFunction("", expression, globals);
		}

		protected HappyRuntimeContext CompileFunction(string parameters, string expression, string globals = null)
		{
			return this.CompileModule((globals != null ? "def " + globals + "; " : "") + @"function testFunc(" + parameters + ") {" + expression + "}");
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
			Assert.AreEqual(0, _languageContext.ErrorSink.ErrorCount, "Wanted zero errors but {0} were found.", _languageContext.ErrorSink.ErrorCount);
		}

		protected string Execute(string snippet, string globals = null)
		{
			return Execute(snippet, null, globals);
		}

		protected string Execute(string snippet, Action<dynamic> globalInitializer, string globals = null)
		{
			string module = (globals != null ? "def " + globals + "; " : "") + "function testFunc() {" + snippet + "}";
			HappyRuntimeContext rc = this.CompileModule(module);
			if(globalInitializer != null)
				globalInitializer(rc.Globals);
			rc.Globals.testFunc();
			return rc.OutputWriter.ToString();
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

		internal void ExpectCompileError(ErrorCode code, string module)
		{
			AssertSyntaxErrorException(code, () => this.CompileModule(module));
		}

		internal static void AssertSyntaxErrorException(ErrorCode errorCode, Action action)
		{
			try
			{
				action();
				Assert.Fail("SyntaxErrorException not thrown as expected");
			}
			catch (SyntaxErrorException e)
			{
				Console.WriteLine(e.StackTrace);
				Assert.AreEqual(errorCode, (ErrorCode)e.ErrorCode);
			}
		}
		#endregion
	}
}
