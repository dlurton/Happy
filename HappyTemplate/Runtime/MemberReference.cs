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
