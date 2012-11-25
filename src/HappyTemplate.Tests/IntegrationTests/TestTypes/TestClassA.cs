/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

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

