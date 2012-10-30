using System;

namespace HappyTemplate.Tests.IntegrationTests.TestTypes
{
	public class TestClass
	{
		public enum TestClassEnum
		{
			A, B, C
		}

		public const int ValueSetToStaticABySomeStaticMethod = 100;
		public const int ValueSetToABySomeInstanceMethod = 101;
		public const int SomeIntConstant = 12345;
		public const string SomeStringConstant = "Hello, World!";
		public int A { get; set; }
		public int B { get; set; }

		public static object[] StaticTestArray = {
			10, 11, 12, 13
		};

		public object[] InstanceTestArray = {
			10, 11, 12, 13
		};


		public static int StaticA { get; set; }

		public TestClass()
		{
			
		}


		public TestClass(int a, int b)
		{
			this.A = a;
			this.B = b;
		}

		public static void SomeStaticMethod()
		{
			StaticA = ValueSetToStaticABySomeStaticMethod;
		}

		public static void SomeStaticMethod(int a)
		{
			StaticA = a; 
		}

		public static void SomeStaticMethod(bool a)
		{
			StaticA = a  ? -1 : 0;
		}

		public static void SomeStaticMethod(string n)
		{
			StaticA = Int32.Parse(n);
		}

		public void SomeInstanceMethod()
		{
			this.A = ValueSetToStaticABySomeStaticMethod;
		}

		public void SomeInstanceMethod(int a)
		{
			this.A = a;
		}

		public void SomeInstanceMethod(bool a)
		{
			this.A = a ? -1 : 0;
		}

		public void SomeInstanceMethod(string n)
		{
			this.A = Int32.Parse(n);
		}
	}
}
