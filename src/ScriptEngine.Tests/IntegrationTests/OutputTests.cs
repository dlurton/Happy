/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Runtime;
using NUnit.Framework;

namespace Happy.ScriptEngine.Tests.IntegrationTests
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

