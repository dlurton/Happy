/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using Happy.ScriptEngine.Compiler;

namespace Happy.ScriptEngine.Exceptions
{
	public class AbortParseException : SourceException
	{
		public AbortParseException(HappySourceLocation loc)
			: base(loc)
		{

		}
	}
}

