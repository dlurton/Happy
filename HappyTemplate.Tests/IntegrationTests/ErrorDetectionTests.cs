using Microsoft.Scripting;
using NUnit.Framework;

namespace HappyTemplate.Tests.IntegrationTests
{
	[TestFixture]
	public class ErrorDetectionTests : TestFixtureBase
	{
		//TODO:  i know there are many other error detection tests.  find them and bring them here.
		[Test]
		public void UnclosedTemplate()
		{
			//TODO:  need to enssure this is the right kind of syntax error exception 
			Assert.Throws<SyntaxErrorException>(() => base.CompileFunction(@"~<|this text is verbatim |% ~"".""; %| aslkjfsdlkjf s"));
		}
		
	}
}
