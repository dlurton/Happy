using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HappyTemplate.Runtime.Trackers
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
