using System;
using System.Dynamic;
using HappyTemplate.Runtime;
using HappyTemplate.Tests.IntegrationTests.TestTypes;
using NUnit.Framework;

namespace HappyTemplate.Tests.IntegrationTests
{
	[TestFixture]
	public class FunctionCallTests : TestFixtureBase
	{
		[Test]
		public void FuncCallsOtherFunc()
		{
			const string moduleToParse = "function callMe() ~<|hi there, I was called!|>;" +
										   "function testFunc() { callMe(); }";
			HappyRuntimeContext rc = CompileModule(moduleToParse);
			rc.Globals.testFunc();
			Assert.AreEqual("hi there, I was called!", rc.OutputWriter.ToString());
		}

		[Test]
		public void FuncCallsOtherFuncWithLiteralArgument()
		{
			const string moduleToParse = @"function callMe(a) ~<|hi there, the value of a is ""|%~a;%|""|>;" +
										   @"function testFunc() ~callMe(""hello, world!"");";

			HappyRuntimeContext rc = CompileModule(moduleToParse);
			rc.Globals.testFunc();
			Assert.AreEqual("hi there, the value of a is \"hello, world!\"", rc.OutputWriter.ToString());
		}
		[Test]
		public void FuncCallsOtherFuncWithVariableArgument()
		{
			const string moduleToParse = "def someGlobal; " +
											"function callMe(a) ~<|hi there, the value of a is \"|%~a;%|\"|>;" +
										   "function testFunc(){callMe(someGlobal);}";

			HappyRuntimeContext rc = CompileModule(moduleToParse);
			rc.Globals.someGlobal = "hello, world!";
			rc.Globals.testFunc();
			Assert.AreEqual("hi there, the value of a is \"hello, world!\"", rc.OutputWriter.ToString());
		}

		[Test]
		public void FuncCallsOtherFuncWithFunctionArgument()
		{
			const string moduleToParse = "function hiWorld() ~<|hello, world!|>;" +
											"function callMe(a) ~<|hi there, the value of a is \"|%~a;%|\"|>;" +
										   "function testFunc() callMe( <||%hiWorld();%||>); ";

			HappyRuntimeContext rc = CompileModule(moduleToParse);
			rc.Globals.testFunc();
			Assert.AreEqual("hi there, the value of a is \"hello, world!\"", rc.OutputWriter.ToString());
		}

		[Test]
		public void FuncCallsOtherFuncWithMultipleMixedArguments()
		{
			const string moduleToParse = "def hiWorld; " +
											"function hi(there) return <|hi |%~there;%|, |>;" +
											"function theValueIs(value) return <|the value is \"|%~value;%|\"|>;" +
											"function callMe(a, b, c) ~a b c; " +
										   "function testFunc() callMe(hi(\"there\"), theValueIs(hiWorld), \"!\");";

			HappyRuntimeContext rc = CompileModule(moduleToParse);
			rc.Globals.hiWorld = "hello, world";
			rc.Globals.testFunc();
			Assert.AreEqual("hi there, the value is \"hello, world\"!", rc.OutputWriter.ToString());
		}

		[Test]
		public void CallCustomLambda()
		{
			const string moduleToParse = @"function testFunc(){ def retval = aCustomLambda(10); ~retval; }";
			HappyRuntimeContext rc = CompileModule(moduleToParse);
			rc.Globals.aCustomLambda = new Func<int, string>((anInteger) => "The value passed was " + anInteger);
			Assert.AreEqual("The value passed was 9", rc.Globals.aCustomLambda(9));
			rc.Globals.testFunc();
			Assert.AreEqual("The value passed was 10", rc.OutputWriter.ToString());
		}

		[Test]
		public void MemberAccess_SimpleCall()
		{
			dynamic someObject = new ExpandoObject();
			someObject.SomeMethod = new Func<dynamic>(() => "Hello member calls!");
			//Not really an instance method call, this is calling a delegate
			AssertOutput("Hello member calls!", "~someObject.SomeMethod();",
				gs => gs.someObject = someObject, "someObject");
		}

		[Test]
		public void MemberAccess_ComplexCall()
		{
			dynamic someObject1 = new ExpandoObject();
			dynamic someObject2 = new ExpandoObject();
			someObject1.SomeProperty = someObject2;
			someObject2.SomeMethod = new Func<dynamic>(() => "Hello member calls!");

			//Not really an instance method call, this is calling a delegate
			AssertOutput("Hello member calls!", "~someObject.SomeProperty.SomeMethod();",
				gs => gs.someObject = someObject1, "someObject");
		}
		[Test]
		public void MemberAccess_SimpleCallWithArgs()
		{
			dynamic someObject = new ExpandoObject();
			someObject.SomeProperty = "Hello member calls!";
			someObject.SomeMethod = new Func<dynamic, dynamic, dynamic>((a, b) =>
			{
				Assert.AreEqual("Aye", a);
				Assert.AreEqual("Bee", b);
				return "Hello member calls!";
			});

			//Not really an instance method call, this is calling a delegate
			AssertOutput("Hello member calls!", @"~someObject.SomeMethod(""Aye"", ""Bee"");",
				gs => gs.someObject = someObject, "someObject");
		}

		[Test]
		public void MemberAccess_ComplexCallWithArgs()
		{
			dynamic someObject1 = new ExpandoObject();
			dynamic someObject2 = new ExpandoObject();
			someObject1.SomeProperty = someObject2;
			someObject2.SomeMethod = new Func<dynamic, dynamic, string>(
				(a, b) =>
				{
					Assert.AreEqual("Aye", a);
					Assert.AreEqual("Bee", b);
					return "Hello member calls!";
				});
			//Not really an instance method call, this is calling a delegate
			AssertOutput("Hello member calls!", @"~someObject.SomeProperty.SomeMethod(""Aye"", ""Bee"");",
				gs => gs.someObject = someObject1, "someObject");
		}	
	}
}
