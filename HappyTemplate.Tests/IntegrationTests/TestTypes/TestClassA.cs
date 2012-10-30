namespace HappyTemplate.Tests.IntegrationTests.TestTypes
{
	class TestClassA
	{
		public string ReadWrite { get; set; }
		public TestClassB NullProperty { get { return null; } }
		public TestClassB NonNullProperty { get { return new TestClassB(); } }
		public TestClassA MyParent { get; set; }
	}
}