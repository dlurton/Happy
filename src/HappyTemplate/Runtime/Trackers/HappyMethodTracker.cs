/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Reflection;

namespace HappyTemplate.Runtime.Trackers
{
	internal class HappyMethodTracker : IHappyTracker
	{
		public MethodInfo[] Methods { get; private set; }
		private readonly HappyTypeTracker _typeTracker;



		public string Name { get; private set; }
		public HappyTypeTracker TypeTracker { get { return _typeTracker;}}

		private string _fullName;
		public string FullName
		{
			get
			{
				if(_fullName == null)
					_fullName = _typeTracker.FullName + "." + this.Name;

				return _fullName;
			}
		}

		public HappyMethodTracker(HappyTypeTracker typeTracker, string name, MethodInfo[] methods)
		{
			this.Methods = methods;
			_typeTracker = typeTracker;
			this.Name = name;
		}
	}
}

