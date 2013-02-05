/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Happy.ScriptEngine.Compiler;
using Happy.ScriptEngine.Exceptions;

namespace Happy.ScriptEngine.Runtime.Trackers
{
	class HappyTypeTracker : IHappyTracker
	{
		readonly Type _myType;
		private readonly Dictionary<string, IHappyTracker> _members = new Dictionary<string, IHappyTracker>();

		public Type Type { get { return _myType; } }

		public string FullName
		{
			get
			{
				return _myType.FullName;
			}

		}
		public string Name
		{
			get { return _myType.Name; }
		}

		public ConstructorInfo[] Constructors
		{
			get
			{
				return _myType.GetConstructors();
			}
		}

		public HappyTypeTracker(Type myType)
		{
			_myType = myType;
		}


		public bool FindMemberTracker(string memberName, out IHappyTracker result)
		{
			lock (_members)
			{
				IHappyTracker tracker;
				if(!_members.TryGetValue(memberName, out tracker))
				{
					MemberInfo[] memberInfos = _myType.GetMember(memberName);

					if(memberInfos.Length == 0)
					{
						result = null;
						return false;
					}
					MemberInfo found = memberInfos[0];
					if(found is MethodInfo) //<--if the first memberInfo is a MethodInfo, they all are MethodInfo
						tracker = new HappyMethodTracker(this, memberName, (from m in memberInfos select (MethodInfo)m).ToArray());
					else if(found is PropertyInfo || found is FieldInfo)
					{
						DebugAssert.IsTrue(memberInfos.Length == 1, "I think I found {0} PropertyInfos.  I really just wanted 1.", memberInfos.Length);
						tracker = new HappyPropertyOrFieldTracker(memberName, found);
					}
					else if(found is Type)
						tracker = new HappyTypeTracker((Type)found);
					else
						throw new UnhandledCaseException();

					_members[memberName] = tracker;

				}
				result = tracker;
			}
			return true;
		}
	}
}

