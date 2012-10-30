using System.Dynamic;
using HappyTemplate.Runtime;
using NUnit.Framework;

namespace HappyTemplate.Tests.IntegrationTests
{
	[TestFixture]
	public class OutputTests : TestFixtureBase
	{
		[Test]
		public void Template()
		{
			AssertOutput("this text is verbatim", "~<|this text is verbatim|>;");
		}

		[Test]
		public void WriteSimpleTemplate()
		{
			AssertOutput("verbatim<dude, this is cool>mitaberv", "~<|verbatim$test$mitaberv|>;",
					globals => globals.test = "<dude, this is cool>", "test");
		}

		[Test]
		public void OutputStatement()
		{
			AssertOutput("I am a test.", @"~<|I $test1$ $test2$ $test3$.|>;",
					globals =>
					{
						globals.test1 = "am";
						globals.test2 = "a";
						globals.test3 = "test";
					}, "test1, test2, test3");
		}
		[Test]
		public void NullVariableHandling()
		{
			HappyRuntimeContext rc = CompileFunction("~aNullValue;", "aNullValue");
			rc.Globals.aNullValue = null;
			rc.Globals.testFunc();
			Assert.AreEqual("", rc.OutputWriter.ToString());
		}
	}
}
