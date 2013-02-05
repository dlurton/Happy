/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Microsoft.Scripting;
using NUnit.Framework;

namespace Happy.ScriptEngine.Tests.IntegrationTests
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

