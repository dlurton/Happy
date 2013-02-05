/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using Happy.ScriptEngine.Compiler;

namespace Happy.ScriptEngine.Exceptions
{
	public class InternalSourceException : SourceException
	{
		public InternalSourceException(HappySourceLocation loc)
			: base(loc) { }
		public InternalSourceException(HappySourceLocation loc, string msg, params object[] args)
			: base(loc, msg, args)  { }

		public InternalSourceException(Exception inner, HappySourceLocation loc, string msg, params object[] args)
			: base(inner, loc, msg, args) { }
	}
}

