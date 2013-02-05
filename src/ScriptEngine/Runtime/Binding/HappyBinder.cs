/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;

namespace Happy.ScriptEngine.Runtime.Binding
{
	class HappyBinder : DefaultBinder
	{

		public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
		{
			return toType.IsAssignableFrom(fromType);
		}

		public override Candidate PreferConvert(Type t1, Type t2)
		{
			throw new NotImplementedException();
		}

	}
}

