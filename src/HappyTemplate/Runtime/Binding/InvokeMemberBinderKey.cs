/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Dynamic;
using Microsoft.Scripting;

namespace HappyTemplate.Runtime.Binding
{
	/// <summary>
	/// This class is needed to canonicalize InvokeMemberBinders in BinderFactory.  See
	/// the comment above the GetXXXBinder methods at the end of the BinderFactory class.
	/// </summary>
	class InvokeMemberBinderKey
	{
		string _name;
		CallInfo _info;

		public InvokeMemberBinderKey(string name, CallInfo info)
		{
			_name = name;
			_info = info;
		}

		public string Name { get { return _name; } }
		public CallInfo Info { get { return _info; } }

		public override bool Equals(object obj)
		{
			InvokeMemberBinderKey key = obj as InvokeMemberBinderKey;
			// Don't lower the name.  BinderFactory is case-preserving in the metadata
			// in case some DynamicMetaObject ignores ignoreCase.  This makes
			// some interop cases work, but the cost is that if a BinderFactory program
			// spells ".foo" and ".Foo" at different sites, they won't share rules.
			return key != null && key._name == _name && key._info.Equals(_info);
		}

		public override int GetHashCode()
		{
			// Stolen from DLR sources when it overrode GetHashCode on binders.
			return 0x28000000 ^ _name.GetHashCode() ^ _info.GetHashCode();
		}

	}
}

