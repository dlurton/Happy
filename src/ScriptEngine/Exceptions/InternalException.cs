/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;

namespace Happy.ScriptEngine.Exceptions
{
    public class InternalException : Exception
    {
        public InternalException(string msg) : base (msg) { }

		public InternalException(string msg, params object[] args)
			: base(String.Format(msg, args)) { }

		public InternalException(Exception inner, string msg, params object[] args)
			: base(String.Format(msg, args), inner) { }

    }
}

