﻿using HappyTemplate.Compiler;
using HappyTemplate.Runtime;
using HappyTemplate.Tests.IntegrationTests.TestTypes;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace HappyTemplate.Tests.IntegrationTests
{
	[TestFixture]
	public class StatementTests : TestFixtureBase
	{
		[Test]
		public void SimpleIfPair()
		{
			AssertOutput("sometrueending", "~<|some|%if(trueValue) { %|true|% } if(falseValue) { %|false|% } %|ending|>;",
				gs =>
				{
					gs.trueValue = true;
					gs.falseValue = false;
				}, "trueValue, falseValue");
		}

		[Test]
		public void SimpleIfFalse()
		{
			AssertOutput("some  value", "~<|some |%if(falseValue) { %|not|% } %| value|>;",
				gs => gs.falseValue = false, "falseValue");

		}

		[Test]
		public void SimpleIfNotFalse()
		{
			AssertOutput("some not false value", "~<|some |%if(!falseValue)%|not|% %| false value|>;",
				gs => gs.falseValue = false, "falseValue");

		}

		[Test]
		public void IfNested()
		{
			const string function = @"
function testFunc(value)
{
	if(value > 1)
	{
		~""gt1"";
		if(value > 2)
			~""gt2"";
	}
}";
			var htc = CompileModule(function);
			htc.Globals.testFunc(1);
			Assert.AreEqual("", htc.OutputWriter.ToString());
			htc.PopWriter();
			htc.PushWriter();

			htc.Globals.testFunc(2);
			Assert.AreEqual("gt1", htc.OutputWriter.ToString());
			htc.PopWriter();
			htc.PushWriter();
			htc.Globals.testFunc(3);
			Assert.AreEqual("gt1gt2", htc.OutputWriter.ToString());
		}

		[Test]
		public void WriteIfElse()
		{
			AssertOutput("some true value", "~<|some |%if(trueValue)%|true|%else%|false|% %| value|>;",
				gs => gs.trueValue = true, "trueValue");

			AssertOutput("some false value", "~<|some |%if(falseValue)%|true|%else%|false|% %| value|>;",
				gs => gs.falseValue = false, "falseValue");

		}
		[Test]
		public void BinaryIf()
		{
			AssertOutput("not", @"if(1 == 2) ~""equal""; else ~""not"";");
			AssertOutput("equal", @"if(2 == 2) ~""equal""; else ~""not"";");

			AssertOutput("not", @"if(""not"" == ""equal"") ~""equal""; else ~""not"";");
			AssertOutput("equal", @"if(""equal"" == ""equal"") ~""equal""; else ~""not"";");

			AssertOutput("equal", @"if(2 != 2) ~""not""; else ~""equal"";");
			AssertOutput("not", @"if(2 != 2) ~""equal""; else ~""not"";");

			AssertOutput("not", @"if(""not"" != ""equal"") ~""not""; else ~""equal"";");
			AssertOutput("equal", @"if(""equal"" != ""equal"") ~""not""; else ~""equal"";");
		}

		[Test]
		public void While_Basic()
		{
			const string script = @"
def i = 0;
while(i < 5)
{
	~i;
	i = i + 1;
}";
			AssertOutput("01234", script);
		}

		[Test]
		public void While_WithBreak()
		{
			const string script = @"
def i = 0;
while(i < 100)
{
	~i;
	i = i + 1;
	if(i >= 5)
		break;
}";
			AssertOutput("01234", script);

		}

		[Test]
		public void While_WithContinue()
		{
			const string script = @"
def i = 0;
while(i < 5)
{
	i = i + 1;
	if(i % 2 != 0)
		continue;
	~i;
}";
			AssertOutput("24", script);
		}

		[Test]
		public void While_WithBreakAndContinue()
		{
			const string script = @"
def i = 0;
while(i < 100)
{
	i = i + 1;
	if(i >= 5)
		break;
	if(i % 2 != 0)
		continue;
	~i;
}";
			AssertOutput("24", script);
		}

		[Test]
		public void For()
		{
			AssertOutput("this is an array", "for(i in array) ~i; ",
				gs => gs.array = new[] { "this ", "is ", "an ", "array" }, "array");
		}

		[Test]
		public void ForNested()
		{
			AssertOutput("1a1b2a2b3a3b4a4b5a5b",
			@"for(i in array1) 
				for(n in array2)
					~<|$i$$n$|>;
			",
			 gs =>
			 {
				 gs.array1 = new[] { 1, 2, 3, 4, 5 };
				 gs.array2 = new[] { "a", "b" };
			 }, "array1, array2");
		}


		[Test]
		public void ForWithBreak()
		{
			AssertOutput("1234",
			@"for(i in array) 
			{
				if(i > 4)
					break; 
				~i;
			}",
				gs => gs.array = new[] { 1, 2, 3, 4, 5 }, "array");
		}

		[Test]
		public void ForWithContinue()
		{
			AssertOutput("246",
			@"for(i in array) 
			{
				if(i % 2 > 0)
					continue; 
				~i;
			}",
				gs => gs.array = new[] { 1, 2, 3, 4, 5, 6 }, "array");
		}

		[Test]
		public void ForWithBetween()
		{
			AssertOutput("this is an array", @"for(i in array between "" "") ~i; ",
				gs => gs.array = new[] { "this", "is", "an", "array" }, "array");
		}

		[Test]
		public void ForWithWhere()
		{
			AssertOutput("246", @"for(i in array where i % 2 == 0) ~i; ",
				gs => gs.array = new[] { 1, 2, 3, 4, 5, 6, 7 }, "array");
		}

		[Test]
		public void ForWithBetweenAndWhere()
		{
			AssertOutput("2,4,6", @"for(i in array where i % 2 == 0 between "","") ~i; ",
				gs => gs.array = new[] { 1, 2, 3, 4, 5, 6, 7 }, "array");
		}

		[Test]
		public void ForWithDefInBlock()
		{
			//We had an error case where variables defined inside of a for's block were always undefined
			AssertOutput("this is an array", "for(i in array) { def a = i; ~a; } ",
				gs => gs.array = new[] { "this ", "is ", "an ", "array" }, "array");
		}
		[Test]
		public void DefWithInitializer()
		{
			HappyRuntimeContext rc = CompileFunction(@"def aVariable = ""aValue""; ~aVariable;");
			rc.Globals.testFunc();
			Assert.AreEqual("aValue", rc.OutputWriter.ToString());
			//ensure aVariable is not part of the global scope
			object dummy;
			ScriptScope ss = rc.Globals;
			Assert.IsFalse(ss.TryGetVariable("aVariable", out dummy));
		}

		[Test]
		public void GlobalDefWithInitializer()
		{
			HappyRuntimeContext rc = CompileModule(@"
def aGlobal = 123;
def anotherGlobal = aGlobal - 23;
");
			Assert.AreEqual(123, rc.Globals.aGlobal);
			Assert.AreEqual(100, rc.Globals.anotherGlobal);
		}

		[Test]
		public void SwitchEmpty()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function testFunc(a)
{
	switch(a)
	{
	}
	return 10;
}
");
			Assert.AreEqual(10, rc.Globals.testFunc("a"));
		}

		[Test]
		public void SwitchEmptyWithDefault()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function testFunc(a)
{
	switch(a)
	{
	default: 
		return 99;
		break;
	}
	return 10;
}
");
			Assert.AreEqual(99, rc.Globals.testFunc("a"));
		}

		[Test]
		public void SwitchBasic()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function testFunc(a)
{
	switch(a)
	{
	case ""a"": 
		return 1;
		break;
	case ""b"": 
		return 2;
		break;
	case ""c"":
		return 3;
		break;
	}

	return 4;
}
");
			Assert.AreEqual(1, rc.Globals.testFunc("a"));
			Assert.AreEqual(2, rc.Globals.testFunc("b"));
			Assert.AreEqual(3, rc.Globals.testFunc("c"));
			Assert.AreEqual(4, rc.Globals.testFunc("d"));
		}

		[Test]
		public void SwitchWithEmptyCase()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function testFunc(a)
{
	switch(a)
	{
	case ""a"": 
		return 1;
		break;
	case ""b"": 
		/*Empty case*/
		break;
	}

	return 2;
}
");
			Assert.AreEqual(1, rc.Globals.testFunc("a"));
			Assert.AreEqual(2, rc.Globals.testFunc("b"));
			Assert.AreEqual(2, rc.Globals.testFunc("c"));
		}

		[Test]
		public void SwitchEnum()
		{
			HappyRuntimeContext rc = CompileModule(
@"
load ""HappyTemplate.Tests"";
use HappyTemplate.Tests.IntegrationTests.TestTypes;
function testFunc(a)
{
	switch(a)
	{
	case TestEnum.One: 
		return 1;
		break;
	case TestEnum.Two: 
		return 2;
		break;
	case TestEnum.Three:
		return 3;
		break;
	}
}
");
			Assert.AreEqual(1, rc.Globals.testFunc(TestEnum.One));
			Assert.AreEqual(2, rc.Globals.testFunc(TestEnum.Two));
			Assert.AreEqual(3, rc.Globals.testFunc(TestEnum.Three));
		}

		[Test]
		public void SwitchCaseWithMultipleValues()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function testFunc(a)
{
	switch(a)
	{
	case ""a"": 
	case ""b"": 
		return 1;
		break;
	case ""c"":
	case ""d"":
	case ""e"":
		return 2;
		break;
	}

	return 3;
}
");
			Assert.AreEqual(1, rc.Globals.testFunc("a"));
			Assert.AreEqual(1, rc.Globals.testFunc("b"));
			Assert.AreEqual(2, rc.Globals.testFunc("c"));
			Assert.AreEqual(2, rc.Globals.testFunc("d"));
			Assert.AreEqual(2, rc.Globals.testFunc("e"));
			Assert.AreEqual(3, rc.Globals.testFunc("f"));
		}

		[Test]
		public void SwitchWithDefault()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function testFunc(a)
{
	switch(a)
	{
	case ""a"": 
		return 1;
		break;
	case ""b"": 
		return 2;
		break;
	default:
		return 3;
		break;
	}

	return 4;
}
");
			Assert.AreEqual(1, rc.Globals.testFunc("a"));
			Assert.AreEqual(2, rc.Globals.testFunc("b"));
			Assert.AreEqual(3, rc.Globals.testFunc("c"));
			Assert.AreEqual(3, rc.Globals.testFunc("d"));
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void SwitchCaseMissingBreak()
		{
			AssertSyntaxErrorException(ErrorCode.SwitchCaseMissingBreak,
			() => CompileModule(@"
function testFunc(a)
{
	switch(a)
	{
	case ""a"": 
		b = 1;
	}

}
"));
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void SwitchDefaultMissingBreak()
		{
			AssertSyntaxErrorException(ErrorCode.SwitchCaseMissingBreak,
				() =>
				{
					HappyRuntimeContext rc = CompileModule(@"
						function testFunc(a)
						{
							switch(a)
							{
							default: 
								b = 1;
							}
						}");
				});
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void SwichWith2Defaults()
		{
			AssertSyntaxErrorException(ErrorCode.DefaultCaseSpecifiedMoreThanOnce,
				() =>
				{
					HappyRuntimeContext rc = CompileModule(@"
					function testFunc(a)
					{
						switch(a)
						{
						default: 
							break;
						default:
							break;
						}
					}
					");
				});
		}

		[Test]
		public void ReturnValue()
		{
			HappyRuntimeContext rc = CompileModule(@"function testFunc(){return 10;}");
			object retval = rc.Globals.testFunc();
			Assert.IsNotNull(retval);
			Assert.IsInstanceOf(typeof(int), retval);
			Assert.AreEqual((int)retval, 10);
		}

		[Test]
		public void ReturnWithoutValue()
		{
			HappyRuntimeContext rc = CompileModule(@"function testFunc(){ return; return 10;}");
			Assert.IsNull(rc.Globals.testFunc());
		}
	}
}

	
