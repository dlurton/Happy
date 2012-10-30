using System;
using System.Collections.Generic;
using System.Dynamic;
using HappyTemplate.Runtime;
using HappyTemplate.Tests.IntegrationTests.TestTypes;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace HappyTemplate.Tests.IntegrationTests
{
	[TestFixture]
	public class ExpressionTests : TestFixtureBase
	{   
		[Test]
		public void LiteralString()
		{
			AssertOutput("this text is a literal", @"~""this text is a literal"";");
		}

		[Test]
		public void LiteralInt()
		{
			AssertOutput("1", "~1;");
		}

		[Test]
		public void SimpleArithmaticLiteralExpressions()
		{
			AssertOutput("3", "~1 + 2;");
			AssertOutput("1", "~2 - 1;");
			AssertOutput("4", "~2 * 2;");
			AssertOutput("3", "~6 / 2;");
			AssertOutput("2", "~6 % 4;");
		}

		[Test]
		public void SimpleArithmaticLiteralAndGlobalVariableExpressions()
		{
			AssertOutput("3", "~a + 2;", globals => globals.A = 1, "a");
			AssertOutput("1", "~a - 1;", globals => globals.A = 2, "a");
			AssertOutput("4", "~a * 2;", globals => globals.A = 2, "a");
			AssertOutput("3", "~a / 2;", globals => globals.A = 6, "a");
			AssertOutput("2", "~a % 4;", globals => globals.A = 6, "a");
		}

		[Test]
		public void SimpleArithmaticLiteralAndLocalVariableExpressions()
		{
			AssertOutput("3", "def a = 1;~a + 2;");
			AssertOutput("1", "def a = 2;~a - 1;");
			AssertOutput("4", "def a = 2;~a * 2;");
			AssertOutput("3", "def a = 6;~a / 2;");
			AssertOutput("2", "def a = 6;~a % 4;");
		}

		[Test]
		public void SimpleBooleanExpressions()
		{
			AssertOutput("True", "~true;");
			AssertOutput("False", "~false;");
			AssertOutput("False", "~false == true;");

			AssertOutput("False", "~!true;");
			AssertOutput("True", "~!false;");
			AssertOutput("True", "~!false == true;");
			AssertOutput("True", "~false == !true;");
			AssertOutput("True", "~!false == !false;");
			AssertOutput("False", "~!(true == true);");
			AssertOutput("True", "~!(false == true);");
		}

		[Test]
		public void SimpleLogicalExpressions()
		{
			AssertOutput("True", "~true || true;");
			AssertOutput("True", "~false || true;");
			AssertOutput("True", "~true || false;");
			AssertOutput("False", "~false || false;");

			AssertOutput("True", "~true && true;");
			AssertOutput("False", "~false && true;");
			AssertOutput("False", "~true && false;");
			AssertOutput("False", "~false && false;");

			AssertOutput("False", "~true ^ true;");
			AssertOutput("True", "~false ^ true;");
			AssertOutput("True", "~true ^ false;");
			AssertOutput("False", "~false ^ false;");
		}

		[Test]
		public void ComplexLogicalExpressions()
		{
			AssertOutput("True", "~1 == 1 || 2 == 2;");
			AssertOutput("True", "~1 == 2 || 2 == 2;");
			AssertOutput("True", "~1 == 1 || 2 == 3;");
			AssertOutput("False", "~1 == 2 || 2 == 3;");

			AssertOutput("True", "~1 == 1 && 2 == 2;");
			AssertOutput("False", "~1 == 2 && 2 == 2;");
			AssertOutput("False", "~1 == 1 && 2 == 3;");
			AssertOutput("False", "~1 == 2 && 2 == 3;");

			AssertOutput("False", "~1 == 1 ^ 2 == 2;");
			AssertOutput("True", "~1 == 2 ^ 2 == 2;");
			AssertOutput("True", "~1 == 1 ^ 2 == 3;");
			AssertOutput("False", "~1 == 2 ^ 2 == 3;");
		}

		[Test]
		public void SimpleBitwiseExpressions()
		{
			AssertOutput("65535", "~0xFFFF ^ 0x0000;");
			AssertOutput("65535", "~0x0000 ^ 0xFFFF;");
			AssertOutput("0", "~0x0000 ^ 0x0000;");
			AssertOutput("0", "~0xFFFF ^ 0xFFFF;");

			AssertOutput("65535", "~0x00FF | 0xFF00;");
			AssertOutput("65535", "~0xFF00 | 0x00FF;");

			AssertOutput("255", "~0x0000 | 0x00FF;");
			AssertOutput("255", "~0x00FF | 0x0000;");

			AssertOutput("3855", "~0xFFFF & 0x0F0F;");
			AssertOutput("3855", "~0x0F0F & 0xFFFF;");

			AssertOutput("0", "~0xFFFF & 0x0000;");
			AssertOutput("0", "~0x0000 & 0xFFFF;");
		}

		[Test]
		public void SimpleHexLiterals()
		{
			AssertOutput("291", "~0x123;");
			AssertOutput("305419896", "~0x12345678;");
			AssertOutput("4886718345", "~0x123456789;");
			AssertOutput("1311768467284833366", "~0x1234567890123456;");
		}

		[Test]
		public void SimpleExpressions()
		{
			AssertOutput("7", "~1 + 2 * 3;");
			AssertOutput("10", "~2 * 3 + 4;");
			AssertOutput("6", "~6 / 2 + 3;");
			AssertOutput("9", "~6 + 9 / 3;");
		}

		[Test]
		public void SimpleExpressionsWithParens()
		{
			AssertOutput("9", "~(1 + 2) * 3;");
			AssertOutput("14", "~2 * (3 + 4);");
			AssertOutput("3", "~15 / (2 + 3);");
			AssertOutput("5", "~(6 + 9) / 3;");
		}

		[Test]
		public void ComplexArithmaticExpressions()
		{
			AssertOutput("18", "~150 % (1 + 2 * 4 + (15 / 5)) * 3;");

			AssertOutput("-10", "~2 * 3 - 16;");
			AssertOutput("-10", "~(2 * 3) - 16;");

			AssertOutput("-110", "~2 - (3 + 4) * 16;");
			AssertOutput("-2", "~2 * 7 - 16;");
			AssertOutput("-2", "~2 * (3 + 4) - 16;");

			AssertOutput("-7", "~2 * (3 + 4) * (5 + 6) / (8 + 9) - 16;");
			AssertOutput("9", "~2 * (3 + 4) * (5 + 6) / (8 + 9);");
			AssertOutput("-8", "~2 * (7) * (11) / (17) - 17;");
			AssertOutput("-8", "~ 2 * (3 + 4) * (5 + 6) / (8 + 9) - 17;");

			//2 * (3 + 4) * (5 + 6) / (8 + 9) - 17 
			//2 * (7) * (11) / (17) - 17 
		}
		[Test]
		public void AndExpressions()
		{
			AssertOutput("False", "~false && false; ");
			AssertOutput("False", "~false && true; ");
			AssertOutput("False", "~true && false; ");
			AssertOutput("True", "~true && true; ");
		}

		[Test]
		public void OrExpressions()
		{
			AssertOutput("False", "~false || false; ");
			AssertOutput("True", "~false || true; ");
			AssertOutput("True", "~true || false; ");
			AssertOutput("True", "~true || true; ");
		}

		[Test]
		public void ComparisonExpressions()
		{
			AssertOutput("True", "~6 <= 6;");
			AssertOutput("True", "~4 <= 6;");
			AssertOutput("False", "~6 <= 4;");
		}

		[Test]
		public void GreaterThanOrEqualExpressions()
		{
			AssertOutput("True", "~6 >= 6;");
			AssertOutput("False", "~4 >= 6;");
			AssertOutput("True", "~6 >= 4;");
		}

		[Test]
		public void LessThanExpressions()
		{
			AssertOutput("False", "~6 < 6;");
			AssertOutput("True", "~4 < 6;");
			AssertOutput("False", "~6 < 4;");
		}

		[Test]
		public void GreaterThanExpressions()
		{
			AssertOutput("False", "~6 > 6;");
			AssertOutput("False", "~4 > 6;");
			AssertOutput("True", "~6 > 4;");
		}

		[Test]
		public void EqualityComparisons()
		{
			AssertOutput("False", "~6 == 4;");
			AssertOutput("True", "~6 == 6;");

			AssertOutput("False", "~6 != 6;");
			AssertOutput("True", "~4 != 6;");
		}


		[Test]
		public void StringLessThanExpressions()
		{
			AssertOutput("False", @"~""a"" < ""a"";");
			AssertOutput("True", @"~""a"" < ""b"";");
			AssertOutput("False", @"~""b"" < ""a"";");
		}

		[Test]
		public void StringGreaterThanExpressions()
		{
			AssertOutput("False", @"~""a"" > ""a"";");
			AssertOutput("True", @"~""b"" > ""a"";");
			AssertOutput("False", @"~""a"" > ""b"";");
		}

		[Test]
		public void StringEqualityComparisons()
		{
			AssertOutput("False", @"~""a"" == ""b"";");
			AssertOutput("True", @"~""a"" == ""a"";");

			AssertOutput("False", @"~""a"" != ""a"";");
			AssertOutput("True", @"~""a"" != ""b"";");
		}

		[Test]
		public void Int32EqualityComparisons()
		{
			AssertOutput("False", @"~6 == 4;");
			AssertOutput("True", @"~6 == 6;");

			AssertOutput("False", @"~6 != 6;");
			AssertOutput("True", @"~4 != 6;");
		}

		[Test]
		public void ReferenceEqualityComparisons()
		{
			AssertOutput("False", @"~6 == 4;");
			AssertOutput("True", @"~6 == 6;");

			AssertOutput("False", @"~6 != 6;");
			AssertOutput("True", @"~4 != 6;");
		}

		[Test]
		public void Assignment()
		{
			HappyRuntimeContext rc = CompileFunction(@" def aVariable; aVariable = ""aValue""; ~aVariable;");
			rc.Globals.testFunc();
			Assert.AreEqual("aValue", rc.OutputWriter.ToString());
			//ensure aVariable is not part of the global scope
			object dummy;
			ScriptScope ss = rc.Globals;
			Assert.IsFalse(ss.TryGetVariable("aVariable", out dummy));
		}

		[Test]
		public void AssignGlobalVariableFromFunction()
		{
			HappyRuntimeContext rc = CompileFunction(
@" 
   aVariable = ""aValue"";
   ~aVariable;
   aVariable = ""aDifferentValue"";
   ~"" "";
   ~aVariable;
", "aVariable");
			rc.Globals.testFunc();
			Assert.AreEqual("aValue aDifferentValue", rc.OutputWriter.ToString());
			Assert.IsInstanceOf(typeof(string), rc.Globals.aVariable);
			Assert.AreEqual("aDifferentValue", rc.Globals.aVariable);
		}

		[Test]
		public void AssignmentFromAnonymousTemplate()
		{
			HappyRuntimeContext rc = CompileModule(
@"
def someVariable;

function testFunc() return ""test template"";

function main()
{
	def aVariable;
	aVariable = <|begin!|%~testFunc();%|!end|>;
	~aVariable;
}
");
			rc.Globals.someVariable = "Some Value";
			rc.Globals.main();
			Assert.AreEqual("begin!test template!end", rc.OutputWriter.ToString());

			//ensure var1 and var2 are not part of the global scope
			object dummy;
			ScriptScope ss = rc.Globals;
			Assert.IsFalse(ss.TryGetVariable("var1", out dummy));
			Assert.IsFalse(ss.TryGetVariable("var2", out dummy));
		}

		[Test]
		public void ReferenceComparison1()
		{
			HappyRuntimeContext rc = CompileFunction(@" if(var1 == var2) ~""equal""; else ~""not equal"";", "var1, var2");
			rc.Globals.var1 = rc.Globals.var2 = new TestClassA();
			rc.Globals.testFunc();
			Assert.AreEqual("equal", rc.OutputWriter.ToString());
		}

		[Test]
		public void ReferenceComparison2()
		{
			HappyRuntimeContext rc = CompileFunction(@" if(var1 == var2) ~""equal""; else ~""not equal"";", "var1, var2");
			rc.Globals.var1 = new TestClassA();
			rc.Globals.var2 = new TestClassA();
			rc.Globals.testFunc();
			Assert.AreEqual("not equal", rc.OutputWriter.ToString());
		}

		[Test]
		public void NullGlobal()
		{
			HappyRuntimeContext rc = CompileModule(@"
def var1 = null;
function main()
{

	if(var1 == null)
		~""null"";
	else
		~""not null"";
}
");
			rc.Globals.main();
			Assert.AreEqual("null", rc.OutputWriter.ToString());
		}

		[Test]
		public void NotNullGlobal()
		{
			HappyRuntimeContext rc = CompileModule(@"
def var1 = null;
function main()
{

	if(var1 != null)
		~""not null"";
	else
		~""null"";
}
");
			rc.Globals.var1 = "nn";
			rc.Globals.main();
			Assert.AreEqual("not null", rc.OutputWriter.ToString());
		}

		[Test]
		public void NullLocal()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function main()
{
	def var1 = null;

	if(var1 == null)
		~""null"";
	else
		~""not null"";
}
");
			rc.Globals.main();
			Assert.AreEqual("null", rc.OutputWriter.ToString());
		}

		[Test]
		public void NotNullLocal()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function main()
{
	def var1 = null;

	if(var1 != null)
		~""not null"";
	else
		~""null"";
}
");
			rc.Globals.main();
			Assert.AreEqual("null", rc.OutputWriter.ToString());
		}

		[Test]
		public void NullReferenceEquality()
		{
			HappyRuntimeContext rc = CompileModule(
@"
function main(p1, p2)
{
	if(p1 == p2)
		return true;
	else
		return false;
}
");
			Assert.AreEqual(true, rc.Globals.main(null, null));
			Assert.AreEqual(false, rc.Globals.main(null, new object()));
			Assert.AreEqual(false, rc.Globals.main(new object(), null));
			Assert.AreEqual(false, rc.Globals.main(new object(), new object()));
		}
		class TestObjectCompare
		{
			private readonly string _value;
			public TestObjectCompare(string v)
			{
				_value = v;
			}
		}

		[Test]
		public void ObjectComparisons()
		{
			HappyRuntimeContext rc = CompileFunction(@"~object1 == object2 "" "" object1 != object2;", "object1, object2");
			rc.Globals.object1 = new TestObjectCompare("A");
			rc.Globals.object2 = new TestObjectCompare("B");

			rc.Globals.testFunc();
			Assert.AreEqual("False True", rc.OutputWriter.ToString());
		}

		[Test]
		public void AnonymousTemplate()
		{
			const string actual = @"
def anon = ""anonymous"";
output = <|I am |%~anon;%|.|>;
~output;
";
			Assert.AreEqual("I am anonymous.", Execute(actual, "output"));
		}
		[Test]
		public void ArrayGetIndex()
		{
			object[] anArray = {
				10, 11, 12, 13
			};
			HappyRuntimeContext context = base.CompileModule(@"
function testFunc1(anArray) { return anArray[0]; }
function testFunc2(anArray) { return anArray[1]; }
function testFunc3(anArray) { return anArray[2]; }
function testFunc4(anArray) { return anArray[3]; }
");
			Assert.AreEqual(10, context.Globals.testFunc1(anArray));
			Assert.AreEqual(11, context.Globals.testFunc2(anArray));
			Assert.AreEqual(12, context.Globals.testFunc3(anArray));
			Assert.AreEqual(13, context.Globals.testFunc4(anArray));
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void ArrayGetIndexOutOfBounds1()
		{
			HappyRuntimeContext context = base.CompileFunction("anArray", @"anArray[1];", null);
			context.Globals.testFunc(new object[] { 10 });
		}
		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void ArrayGetIndexOutOfBounds2()
		{
			HappyRuntimeContext context = base.CompileFunction("anArray", @"anArray[-1];", null);
			context.Globals.testFunc(new object[] { 10 });
		}

		[Test]
		public void ArrayGet2DIndex()
		{
			object[,] anArray = {
				{ 10, 11, 12, 13 },
				{ 20, 21, 22, 23 },
				{ 30, 31, 32, 33 }
			};
			HappyRuntimeContext context = base.CompileModule(@"
function testFunc1(anArray) { return anArray[0, 0]; }
function testFunc2(anArray) { return anArray[0, 1]; }
function testFunc3(anArray) { return anArray[0, 2]; }
function testFunc4(anArray) { return anArray[0, 3]; }

function testFunc5(anArray) { return anArray[1, 0]; }
function testFunc6(anArray) { return anArray[1, 1]; }
function testFunc7(anArray) { return anArray[1, 2]; }
function testFunc8(anArray) { return anArray[1, 3]; }

function testFunc9(anArray) { return anArray[2, 0]; }
function testFunc10(anArray) { return anArray[2, 1]; }
function testFunc11(anArray) { return anArray[2, 2]; }
function testFunc12(anArray) { return anArray[2, 3]; }
");
			Console.WriteLine("Compiled!");
			Assert.AreEqual(10, context.Globals.testFunc1(anArray));
			Console.WriteLine("1");
			Assert.AreEqual(11, context.Globals.testFunc2(anArray));
			Assert.AreEqual(12, context.Globals.testFunc3(anArray));
			Assert.AreEqual(13, context.Globals.testFunc4(anArray));

			Assert.AreEqual(20, context.Globals.testFunc5(anArray));
			Assert.AreEqual(21, context.Globals.testFunc6(anArray));
			Assert.AreEqual(22, context.Globals.testFunc7(anArray));
			Assert.AreEqual(23, context.Globals.testFunc8(anArray));

			Assert.AreEqual(30, context.Globals.testFunc9(anArray));
			Assert.AreEqual(31, context.Globals.testFunc10(anArray));
			Assert.AreEqual(32, context.Globals.testFunc11(anArray));
			Assert.AreEqual(33, context.Globals.testFunc12(anArray));

			Console.WriteLine("Success!");
		}

		

		[Test]
		public void ListGetIndex()
		{
			List<object> aList = new List<object> { 10, 11, 12, 13 };
			HappyRuntimeContext context = base.CompileModule(@"
function testFunc1(aList) { return aList[0]; }
function testFunc2(aList) { return aList[1]; }
function testFunc3(aList) { return aList[2]; }
function testFunc4(aList) { return aList[3]; }
");
			Assert.AreEqual(10, context.Globals.testFunc1(aList));
			Assert.AreEqual(11, context.Globals.testFunc2(aList));
			Assert.AreEqual(12, context.Globals.testFunc3(aList));
			Assert.AreEqual(13, context.Globals.testFunc4(aList));
		}

		//*********************************
		[Test]
		public void ArraySetIndex()
		{
			object[] anArray = new object[4];
			HappyRuntimeContext context = base.CompileModule(@"function testFunc(anArray, anIndex, aValue) { anArray[anIndex] = aValue; }");
			context.Globals.testFunc(anArray, 0, 10);
			context.Globals.testFunc(anArray, 1, 11);
			context.Globals.testFunc(anArray, 2, 12);
			context.Globals.testFunc(anArray, 3, 13);
			Assert.AreEqual(10, anArray[0]);
			Assert.AreEqual(11, anArray[1]);
			Assert.AreEqual(12, anArray[2]);
			Assert.AreEqual(13, anArray[3]);
		}

		[Test]
		public void ArraySetIndexOutOfBounds1()
		{
			HappyRuntimeContext context = base.CompileFunction("anArray", @"anArray[1] = 99;", null);
			Assert.Throws<IndexOutOfRangeException>(() => context.Globals.testFunc(new object[] { 10 }));
		}
		[Test]
		public void ArraySetIndexOutOfBounds2()
		{
			HappyRuntimeContext context = base.CompileFunction("anArray", @"anArray[-1] = 99;", null);
			Assert.Throws<IndexOutOfRangeException>(() => context.Globals.testFunc(new object[] { 10 }));
		}

		[Test]
		public void ArraySet2DIndex()
		{
			object[,] anArray = new object[3, 4];
			HappyRuntimeContext context = base.CompileModule(@"function testFunc(anArray, x, y, value) { anArray[x, y] = value; }");

			context.Globals.testFunc(anArray, 0, 0, 10);
			context.Globals.testFunc(anArray, 0, 1, 11);
			context.Globals.testFunc(anArray, 0, 2, 12);
			context.Globals.testFunc(anArray, 0, 3, 13);

			context.Globals.testFunc(anArray, 1, 0, 20);
			context.Globals.testFunc(anArray, 1, 1, 21);
			context.Globals.testFunc(anArray, 1, 2, 22);
			context.Globals.testFunc(anArray, 1, 3, 23);

			context.Globals.testFunc(anArray, 2, 0, 30);
			context.Globals.testFunc(anArray, 2, 1, 31);
			context.Globals.testFunc(anArray, 2, 2, 32);
			context.Globals.testFunc(anArray, 2, 3, 33);

			Assert.AreEqual(10, anArray[0, 0]);
			Assert.AreEqual(11, anArray[0, 1]);
			Assert.AreEqual(12, anArray[0, 2]);
			Assert.AreEqual(13, anArray[0, 3]);

			Assert.AreEqual(20, anArray[1, 0]);
			Assert.AreEqual(21, anArray[1, 1]);
			Assert.AreEqual(22, anArray[1, 2]);
			Assert.AreEqual(23, anArray[1, 3]);

			Assert.AreEqual(30, anArray[2, 0]);
			Assert.AreEqual(31, anArray[2, 1]);
			Assert.AreEqual(32, anArray[2, 2]);
			Assert.AreEqual(33, anArray[2, 3]);
		}
		

		[Test]
		public void ListSetIndex()
		{
			List<object> aList = new List<object> { 10, 11, 12, 13 };
			HappyRuntimeContext context = base.CompileModule(@"function testFunc1(aList) { return aList[0]; }
function testFunc2(aList) { return aList[1]; }
function testFunc3(aList) { return aList[2]; }
function testFunc4(aList) { return aList[3]; }
");
			Assert.AreEqual(10, context.Globals.testFunc1(aList));
			Assert.AreEqual(11, context.Globals.testFunc2(aList));
			Assert.AreEqual(12, context.Globals.testFunc3(aList));
			Assert.AreEqual(13, context.Globals.testFunc4(aList));
		}
		[Test]
		public void ListIndexOutOfBounds1()
		{
			HappyRuntimeContext context = base.CompileFunction("aList", @"aList[1];", null);
			Assert.Throws<IndexOutOfRangeException>(() => context.Globals.testFunc(new object[] { 10 }));
		}
		[Test]
		public void ListIndexOutOfBounds2()
		{
			HappyRuntimeContext context = base.CompileFunction("aList", @"aList[-1];", null);
			Assert.Throws<IndexOutOfRangeException>(() => context.Globals.testFunc(new object[] { 10 }));
		}

		[Test]
		public void MemberAccess_SimpleGet()
		{
			dynamic someObject = new ExpandoObject();
			someObject.SomeProperty = "Hi properties!";
			AssertOutput("Hi properties!", "~someObject.SomeProperty;",
				gs => gs.someObject = someObject, "someObject");			
		}

		[Test]
		public void MemberAccess_SimpleSet()
		{
			dynamic someObject = new ExpandoObject();
			base.Execute(@"someObject.SomeProperty = ""Hi properties!"";",
				gs => gs.someObject = someObject, "someObject");
			Assert.AreEqual("Hi properties!", someObject.SomeProperty);
		}
		[Test]
		public void MemberAccess_ComplexGet()
		{
			dynamic someObject1 = new ExpandoObject();
			dynamic someObject2 = new ExpandoObject();
			someObject1.SomeProperty = someObject2;
			someObject2.AnotherProperty = "Hi properties!";
			AssertOutput("Hi properties!", "~someObject.SomeProperty.AnotherProperty;",
				gs => gs.someObject = someObject1, "someObject");
		}

		[Test]
		public void MemberAccess_ComplexSet()
		{
			dynamic someObject1 = new ExpandoObject();
			dynamic someObject2 = new ExpandoObject();
			someObject1.SomeProperty = someObject2;
			base.Execute(@"someObject.SomeProperty.AnotherProperty = ""Hi properties!"";",
				gs => gs.someObject = someObject1, "someObject");
			Assert.AreEqual("Hi properties!", someObject2.AnotherProperty);
		}
	}
}
