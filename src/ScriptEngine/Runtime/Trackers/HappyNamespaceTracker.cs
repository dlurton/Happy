/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Happy.ScriptEngine.Runtime.Trackers
{
    public class HappyNamespaceTracker : IHappyTracker, IEnumerable<IHappyTracker>
	{
		private readonly string _name;
		public string Name { get { return _name; } }
		private string _fullName;
    	public HappyNamespaceTracker _parent;
		readonly Dictionary<string, IHappyTracker> _members = new Dictionary<string, IHappyTracker>();

		public HappyNamespaceTracker(string name) : this(null, name) { }

		public HappyNamespaceTracker(HappyNamespaceTracker parent, string name)
		{
			_name = name;
			_parent = parent;
		}

	    public override string ToString()
		{
			return _name;
		}

		/// <summary>
		/// THIS ENUMERATOR IS NOT THREAD SAFE
		/// </summary>
    	public IEnumerator<IHappyTracker> GetEnumerator()
    	{
    		return _members.Values.GetEnumerator();
    	}

		/// <summary>
		/// THIS ENUMERATOR IS NOT THREAD SAFE
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
    	{
			return _members.Values.GetEnumerator();
    	}

    	private void RecurseAppendName(StringBuilder sb)
		{
			if (_parent != null)
				_parent.RecurseAppendName(sb);

			if(sb.Length != 0)
				sb.Append('.');
			sb.Append(this.Name);
		}
    	
		public string FullName 
		{ 
			get
			{
				if(_fullName == null)
				{
					StringBuilder sb = new StringBuilder();
					this.RecurseAppendName(sb);
					_fullName = sb.ToString();
				}
				return _fullName;
			}
		}

		internal dynamic SetMember(string name, IHappyTracker value)
		{
			lock (_members)
			{
				_members[name] = value;
			}
			return value;
		}

		internal bool TryGetMember(string memberName, out IHappyTracker result)
		{
			lock (_members)
			{
				IHappyTracker ns;
				bool success = _members.TryGetValue(memberName, out ns);
				result = ns;
				return success;
			}
		}

		internal bool HasMember(string name)
		{
			lock (_members)
			{
				return _members.ContainsKey(name);
			}
		}

		internal IHappyTracker GetMember(string name)
		{
			lock (_members)
			{
				return _members[name];
			}
		}

		//public HappyNamespaceTracker FindNestedNamespace(IEnumerable<string> segments)
		//{
		//	HappyNamespaceTracker current = this;

		//	foreach(string segment in segments)
		//		current = Util.CastAssert<HappyNamespaceTracker>(current.GetMember(segment));

		//	return current;
		//}
	    public void ForEachMember(Action<IHappyTracker> func)
	    {
		    lock(_members)
		    {
				foreach (var m in _members.Values)
					func(m);
		    }
	    }
	}
}

