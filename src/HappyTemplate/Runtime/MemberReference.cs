/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Actions;

namespace HappyTemplate.Runtime
{
	public class MemberReference : DynamicObject
	{
		private MemberTracker _memberTracker;
		public MemberReference(MemberTracker tracker)
		{
			_memberTracker = tracker;
		}

		//public override bool FindMemberTracker(GetMemberBinder binder, out object result)
		//{
		//    _memberTracker.
		//}
	}
}

