/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Reflection;

namespace Happy.ScriptEngine.Runtime.Trackers
{
	class HappyPropertyOrFieldTracker : IHappyTracker
	{
		public string Name { get; private set; }
		public MemberInfo MemberInfo { get; private set; }
		public HappyPropertyOrFieldTracker(string name, MemberInfo memberInfo)
		{
			if (!(memberInfo is PropertyInfo || memberInfo is FieldInfo))
				throw new ArgumentException("Must be an instance of PropertyInfo or FieldInfo","memberInfo");

			this.Name = name;
			this.MemberInfo = memberInfo;
		}
	}
}

