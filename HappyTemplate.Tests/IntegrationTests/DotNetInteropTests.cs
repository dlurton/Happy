using System;
using System.IO;
using HappyTemplate.Runtime;
using HappyTemplate.Runtime.Trackers;
using Microsoft.Scripting;
using NUnit.Framework;

namespace HappyTemplate.Tests.IntegrationTests.TestTypes.IntegrationTests
{
	[TestFixture]
	public class DotNetInteropTests : TestFixtureBase
	{
		[Test]
		public void ReadSimpleSystemTypes()
		{
			const string moduleToParse = @"
function testFunc1(){ return System.Int32; }
function testFunc2(){ return System.IO.TextWriter; }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);
			Assert.AreEqual(typeof(Int32), rc.Globals.testFunc1().Type);
			Assert.AreEqual(typeof(TextWriter), rc.Globals.testFunc2().Type);
		}

		[Test]
		public void RootNamespace()
		{
			HappyRuntimeContext context = base.CompileModule(@"
function getSystem() { return System; } 
function getMicrosoft() { return Microsoft; }
");

			Assert.IsInstanceOf(typeof(HappyNamespaceTracker), context.Globals.getSystem());
			Assert.IsInstanceOf(typeof(HappyNamespaceTracker), context.Globals.getMicrosoft());
		}

		[Test]
		public void DeepNamespaces()
		{
			HappyRuntimeContext context = base.CompileModule(@"
function testFunc1() { return System.Diagnostics; }
function testFunc2() { return System.Diagnostics.Eventing; }
function testFunc3() { return System.Diagnostics.Eventing.Reader; }
");
			HappyNamespaceTracker result = context.Globals.testFunc1();
			Assert.AreEqual("System.Diagnostics", result.FullName);

			result = context.Globals.testFunc2();
			Assert.AreEqual("System.Diagnostics.Eventing", result.FullName);

			result = context.Globals.testFunc3();
			Assert.AreEqual("System.Diagnostics.Eventing.Reader", result.FullName);
		}

		[Test]
		public void TypeReference()
		{
			HappyRuntimeContext context = base.CompileModule(@"
function testFunc() { return System.Int32; }
");
			Assert.IsInstanceOf(typeof(HappyTypeTracker), context.Globals.testFunc());
		}

		[Test]
		public void TypeAlias()
		{
			HappyRuntimeContext context = base.CompileModule(@"
def Int32 = System.Int32;
function testFunc() { return Int32; }
");
			Assert.IsInstanceOf(typeof(HappyTypeTracker), context.Globals.testFunc());
		}

		[Test]
		public void LocalAssembly()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass; }");
			HappyTypeTracker tracker = context.Globals.testFunc();

			Assert.AreEqual("HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass", tracker.FullName);
		}
		[Test]
		public void StaticMemberInvoke()
		{
			HappyRuntimeContext context = base.CompileExpression(@"System.Int32.Parse(""123"");");
			Assert.AreEqual(123, context.Globals.testFunc());
		}


		[Test]
		public void StaticMemberInvoke2()
		{
			HappyRuntimeContext context = base.CompileModule(
@"function testFunc(input)
{ return System.Int32.Parse(input); }
");
			object returnValue = context.Globals.testFunc("123");
			Assert.IsInstanceOf(typeof(Int32), returnValue);
			Assert.AreEqual(123, returnValue);
		}

		[Test]
		public void StaticMemberInvoke3()
		{
			StringWriter writer = new StringWriter();
			HappyRuntimeContext context = base.CompileModule(
@"function testFunc(input)
{ System.Console.Write(""Hello, {0}!"", input); }
");
			Console.SetOut(writer);
			context.Globals.testFunc("world");
			Assert.AreEqual("Hello, world!", writer.ToString());
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void InvalidNamespace()
		{
			HappyRuntimeContext context = base.CompileModule(
@"
function testFunc()
{ return System.Invalid; }
");
			Assert.Throws<MissingMemberException>(() => context.Globals.testFunc());
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void InvalidMember()
		{
			HappyRuntimeContext context = base.CompileModule(
@"function testFunc()
{ return System.Console.InvalidMember; }
");
			Assert.Throws<MissingMemberException>(() => context.Globals.testFunc());
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void MssingStaticMethod()
		{
			HappyRuntimeContext context = base.CompileModule(
@"load ""HappyTemplate.Tests"";
function testFunc()
{ return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.NonExistentMethod(); }
");
			Assert.Throws<MissingMethodException>(() => context.Globals.testFunc());
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void MissingInstanceMethod()
		{
			HappyRuntimeContext context = base.CompileModule("function testFunc(instance) { return instance.NonExistentMethod(); }");
			Assert.Throws<MissingMethodException>(() => context.Globals.testFunc(new object()));
		}

		[Test]
		public void CantFindStaticOverride()
		{
			HappyRuntimeContext context = base.CompileModule(
@"load ""HappyTemplate.Tests"";
function testFunc()
{ return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStaticMethod(1.0); }
");
			Assert.Throws<MissingMemberException>(() => context.Globals.testFunc());
		}

		[Test]
		[Category(TestCategories.ErrorHandlingTest)]
		public void CantFindInstanceOverride()
		{
			HappyRuntimeContext context = base.CompileModule("function testFunc(instance){ return instance.SomeInstanceMethod(1.0); }");
			Assert.Throws<ArgumentTypeException>(() => context.Globals.testFunc(new TestClass()));
		}

		[Test]
		public void SetStaticProperty()
		{
			TestClass.StaticA = 0;
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticA = 111; }");
			context.Globals.testFunc();
			Assert.AreEqual(111, TestClass.StaticA);
		}

		[Test]
		public void GetStaticProperty()
		{
			TestClass.StaticA = 222;
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticA; }");
			Assert.AreEqual(222, context.Globals.testFunc());
		}

		[Test]
		public void GetIntConstant()
		{
			TestClass.StaticA = 222;
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeIntConstant; }");
			Assert.AreEqual(TestClass.SomeIntConstant, context.Globals.testFunc());
		}

		[Test]
		public void GetStringConstant()
		{
			TestClass.StaticA = 222;
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStringConstant; }");
			Assert.AreEqual(TestClass.SomeStringConstant, context.Globals.testFunc());
		}


		[Test]
		public void ReadValueTypedStaticProperty()
		{
			TestClass.StaticA = 222;
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticA; }");
			Assert.AreEqual(222, context.Globals.testFunc());
		}

		[Test]
		public void ReadValueTypedInstanceProperty()
		{
			HappyRuntimeContext context = base.CompileModule(@"function testFunc(arg) { return arg.A; }");
			dynamic t = context.Globals.testFunc(new TestClass { A = 222 });
			Assert.AreEqual(222, t);
		}

		[Test]
		public void WriteValueTypedInstanceProperty()
		{
			HappyRuntimeContext context = base.CompileModule(@"function testFunc(arg) { ~arg.A; }");
			context.Globals.testFunc(new TestClass { A = 222 });
			Assert.AreEqual("222", context.OutputWriter.ToString());
		}

		[Test]
		public void NewObject()
		{
			HappyRuntimeContext context = base.CompileFunction(" return new(System.Object); ");
			object temp = context.Globals.testFunc();
			Assert.AreEqual(typeof(object), temp.GetType());
		}

		[Test]
		[ExpectedException(typeof(MissingMemberException))]
		public void NewInt32()
		{
			HappyRuntimeContext context = base.CompileFunction(" return new(System.Int32); ");
			object temp = context.Globals.testFunc();
		}

		[Test]
		public void NewTestClass()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return new(HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass); }
");
			object temp = context.Globals.testFunc();
			Assert.AreEqual(typeof(TestClass), temp.GetType());
		}

		[Test]
		public void NewTestClassWithConstructorArgs()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return new(HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass, 1, 2); }
");
			object temp = context.Globals.testFunc();
			Assert.AreEqual(1, ((TestClass)temp).A);
			Assert.AreEqual(2, ((TestClass)temp).B);

		}

		[Test]
		public void NewTestStruct()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return new(HappyTemplate.Tests.IntegrationTests.TestTypes.TestStruct2, 1, 2); }
");
			object temp = context.Globals.testFunc();
			Assert.AreEqual(1, ((TestStruct2)temp).A);
			Assert.AreEqual(2, ((TestStruct2)temp).B);
		}

		[Test]
		public void GetEnum()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestEnum; }
");
			object temp = context.Globals.testFunc();
			Assert.AreEqual(typeof(HappyTypeTracker), temp.GetType());
		}
		[Test]
		public void GetEnumValues()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc0() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestEnum.Zero; }
function testFunc1() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestEnum.One; }
function testFunc2() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestEnum.Two; }
");
			Assert.AreEqual(TestEnum.Zero, context.Globals.testFunc0());
			Assert.AreEqual(TestEnum.One, context.Globals.testFunc1());
			Assert.AreEqual(TestEnum.Two, context.Globals.testFunc2());
		}

		[Test]
		public void GetClassEnum()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.TestClassEnum; }
");
			object temp = context.Globals.testFunc();
			Assert.AreEqual(typeof(HappyTypeTracker), temp.GetType());
		}
		[Test]
		public void GetClassEnumValues()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFuncA() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.TestClassEnum.A; }
function testFuncB() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.TestClassEnum.B; }
function testFuncC() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.TestClassEnum.C; }
");
			Assert.AreEqual(TestClass.TestClassEnum.A, context.Globals.testFuncA());
			Assert.AreEqual(TestClass.TestClassEnum.B, context.Globals.testFuncB());
			Assert.AreEqual(TestClass.TestClassEnum.C, context.Globals.testFuncC());
		}

		[Test]
		public void Use()
		{
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
use System;
use HappyTemplate.Tests.IntegrationTests.TestTypes;
function getTestClass()  { 	return TestClass; }
function getInt32() { return Int32; }
function getString() { return String; } 
");
			Action<string, object> assertTypeTracker = (expectedName, obj) =>
				{
					Assert.IsInstanceOf(typeof (HappyTypeTracker), obj);
					HappyTypeTracker htt = (HappyTypeTracker) obj;
					Assert.AreEqual(expectedName, htt.Name);
				};

			assertTypeTracker("TestClass", context.Globals.getTestClass());
			assertTypeTracker("Int32", context.Globals.getInt32());
			assertTypeTracker("String", context.Globals.getString());
		}
		[Test]
		public void InstancePropertyGetArrayIndex()
		{
			TestClass testObject = new TestClass { InstanceTestArray = new object[] { 10, 11, 12, 13 } };

			HappyRuntimeContext context = base.CompileModule(@"
function testFunc1(testObject) { return testObject.InstanceTestArray[0]; }
function testFunc2(testObject) { return  testObject.InstanceTestArray[1]; }
function testFunc3(testObject) { return  testObject.InstanceTestArray[2]; }
function testFunc4(testObject) { return  testObject.InstanceTestArray[3]; }
");
			Assert.AreEqual(10, context.Globals.testFunc1(testObject));
			Assert.AreEqual(11, context.Globals.testFunc2(testObject));
			Assert.AreEqual(12, context.Globals.testFunc3(testObject));
			Assert.AreEqual(13, context.Globals.testFunc4(testObject));
		}

		[Test]
		public void StaticPropertyGetArrayIndex()
		{
			TestClass.StaticTestArray = new object[] { 10, 11, 12, 13 };
			HappyRuntimeContext context = base.CompileModule(@"
load ""HappyTemplate.Tests"";
function testFunc1() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticTestArray[0]; }
function testFunc2() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticTestArray[1]; }
function testFunc3() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticTestArray[2]; }
function testFunc4() { return HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticTestArray[3]; }
");
			Assert.AreEqual(10, context.Globals.testFunc1());
			Assert.AreEqual(11, context.Globals.testFunc2());
			Assert.AreEqual(12, context.Globals.testFunc3());
			Assert.AreEqual(13, context.Globals.testFunc4());
		}
		[Test]
		public void SimpleMemberAccess()
		{
			HappyRuntimeContext rc = CompileFunction("~testObject.ReadWrite;", "testObject");
			rc.Globals.testObject = new TestClassA() { ReadWrite = "Hello, I'm a member!" };
			rc.Globals.testFunc();
			Assert.AreEqual("Hello, I'm a member!", rc.OutputWriter.ToString());
		}

		[Test]
		public void ComplexMemberAccess()
		{
			HappyRuntimeContext rc = CompileFunction("~testObject.MyParent.ReadWrite;", "testObject");
			rc.Globals.testObject = new TestClassA()
			{
				MyParent = new TestClassA()
				{
					ReadWrite = "Hello, I'm a member of the parent!",
				}

			};
			rc.Globals.testFunc();
			Assert.AreEqual("Hello, I'm a member of the parent!", rc.OutputWriter.ToString());
		}
		[Test]
		public void InstancePropertySetArrayIndex()
		{
			TestClass testObject = new TestClass { InstanceTestArray = new object[4] };

			HappyRuntimeContext context = base.CompileModule(@"function testFunc(testObject, index, value) { testObject.InstanceTestArray[index] = value; }");
			context.Globals.testFunc(testObject, 0, 10);
			context.Globals.testFunc(testObject, 1, 11);
			context.Globals.testFunc(testObject, 2, 12);
			context.Globals.testFunc(testObject, 3, 13);

			Assert.AreEqual(10, testObject.InstanceTestArray[0]);
			Assert.AreEqual(11, testObject.InstanceTestArray[1]);
			Assert.AreEqual(12, testObject.InstanceTestArray[2]);
			Assert.AreEqual(13, testObject.InstanceTestArray[3]);
		}
		/// <summary>
		/// Tests handling when a property that is a scalar value is null
		/// </summary>
		[Test]
		public void NullObjectProperty1()
		{
			HappyRuntimeContext rc = CompileFunction("~objectA.NullProperty;", "objectA");
			rc.Globals.objectA = new TestClassA();
			rc.Globals.testFunc();
			Assert.AreEqual("", rc.OutputWriter.ToString());
		}

		/// <summary>
		/// Tests handling when a property that is an object is null
		/// </summary>
		[Test]
		public void NullObjectProperty2()
		{
			HappyRuntimeContext rc = CompileFunction("~objectA.NonNullProperty.NullProperty;", "objectA");
			rc.Globals.objectA = new TestClassA();
			rc.Globals.testFunc();
			Assert.AreEqual("", rc.OutputWriter.ToString());
		}

		[Test]
		public void StaticPropertySetArrayIndex()
		{
			TestClass.StaticTestArray = new object[4];

			HappyRuntimeContext context = base.CompileModule(@"
			load ""HappyTemplate.Tests"";
			function testFunc(index, value) { HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.StaticTestArray[index] = value; }");
			context.Globals.testFunc(0, 10);
			context.Globals.testFunc(1, 11);
			context.Globals.testFunc(2, 12);
			context.Globals.testFunc(3, 13);

			Assert.AreEqual(10, TestClass.StaticTestArray[0]);
			Assert.AreEqual(11, TestClass.StaticTestArray[1]);
			Assert.AreEqual(12, TestClass.StaticTestArray[2]);
			Assert.AreEqual(13, TestClass.StaticTestArray[3]);
		}
		[Test]
		public void CallOverloadedStaticMethodWithNoParameters()
		{
			const string moduleToParse = @"
load ""HappyTemplate.Tests"";
function testFunc1(){ HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStaticMethod(); }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);

			TestClass.StaticA = 0;
			rc.Globals.testFunc1();
			Assert.AreEqual(TestClass.ValueSetToStaticABySomeStaticMethod, TestClass.StaticA);
		}

		[Test]
		public void CallOverloadedStaticMethodWithIntParameter()
		{
			const string moduleToParse = @"
load ""HappyTemplate.Tests"";
function testFunc1(){ HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStaticMethod(123); }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);

			TestClass.StaticA = 0;
			rc.Globals.testFunc1();
			Assert.AreEqual(123, TestClass.StaticA);
		}

		[Test]
		public void CallOverloadedStaticMethodWithBoolParameter()
		{
			const string moduleToParse = @"
load ""HappyTemplate.Tests"";
function testFunc1(){ HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStaticMethod(true); }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);

			TestClass.StaticA = 0;
			rc.Globals.testFunc1();
			Assert.AreEqual(-1, TestClass.StaticA);
		}

		[Test]
		public void CallOverloadedStaticMethodWithStringParameter()
		{
			const string moduleToParse = @"
load ""HappyTemplate.Tests"";
function testFunc1(){ HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStaticMethod(""1143""); }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);

			TestClass.StaticA = 0;
			rc.Globals.testFunc1();
			Assert.AreEqual(1143, TestClass.StaticA);
		}

		[Test]
		public void CallOverloadedStaticMethodWithDifferentParameters()
		{
			const string moduleToParse = @"
load ""HappyTemplate.Tests"";
function testFunc1(parameter){ HappyTemplate.Tests.IntegrationTests.TestTypes.TestClass.SomeStaticMethod(parameter); }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);

			TestClass.StaticA = 0;


			rc.Globals.testFunc1(true);
			Assert.AreEqual(-1, TestClass.StaticA);

			rc.Globals.testFunc1(101);
			Assert.AreEqual(101, TestClass.StaticA);

			rc.Globals.testFunc1(false);
			Assert.AreEqual(0, TestClass.StaticA);

			rc.Globals.testFunc1("333");
			Assert.AreEqual(333, TestClass.StaticA);
		}

		[Test]
		public void CallOverloadedStaticMethodWithVariableParameters()
		{
			const string moduleToParse = @"
function testFunc1(){ return System.String.Format(""{0}|{1}"", 1, 2); }
";
			HappyRuntimeContext rc = CompileModule(moduleToParse);

			TestClass.StaticA = 0;
			rc.Globals.testFunc1();
			Assert.AreEqual("1|2", rc.Globals.testFunc1());
		}

		/****/
		[Test]
		public void CallOverloadedInstanceMethodWithNoParameters()
		{
			const string moduleToParse = @"function testFunc1(instance){ instance.SomeInstanceMethod(); }";
			HappyRuntimeContext rc = CompileModule(moduleToParse);
			TestClass tc = new TestClass();
			rc.Globals.testFunc1(tc);
			Assert.AreEqual(TestClass.ValueSetToStaticABySomeStaticMethod, tc.A);
		}

		[Test]
		public void CallOverloadedInstanceMethodWithIntParameter()
		{
			const string moduleToParse = @"function testFunc1(instance){ instance.SomeInstanceMethod(123); }"; HappyRuntimeContext rc = CompileModule(moduleToParse);
			TestClass tc = new TestClass();
			rc.Globals.testFunc1(tc);
			Assert.AreEqual(123, tc.A);
		}

		[Test]
		public void CallOverloadedInstanceMethodWithBoolParameter()
		{
			const string moduleToParse = @"function testFunc1(instance){ instance.SomeInstanceMethod(true); }"; HappyRuntimeContext rc = CompileModule(moduleToParse);
			TestClass tc = new TestClass();
			rc.Globals.testFunc1(tc);
			Assert.AreEqual(-1, tc.A);
		}

		[Test]
		public void CallOverloadedInstanceMethodWithStringParameter()
		{
			const string moduleToParse = @"function testFunc1(instance){ instance.SomeInstanceMethod(""8675309""); }";
			HappyRuntimeContext rc = CompileModule(moduleToParse);
			TestClass tc = new TestClass();
			rc.Globals.testFunc1(tc);
			Assert.AreEqual(8675309, tc.A);

		}

		[Test]
		public void CallOverloadedInstanceMethodWithDifferentParameters()
		{
			const string moduleToParse = @"function testFunc1(instance, arg){ instance.SomeInstanceMethod(arg); }";
			HappyRuntimeContext rc = CompileModule(moduleToParse);
			TestClass tc = new TestClass();

			rc.Globals.testFunc1(tc, true);
			Assert.AreEqual(-1, tc.A);

			rc.Globals.testFunc1(tc, 101);
			Assert.AreEqual(101, tc.A);

			rc.Globals.testFunc1(tc, false);
			Assert.AreEqual(0, tc.A);

			rc.Globals.testFunc1(tc, "333");
			Assert.AreEqual(333, tc.A);
		}
	}
}
