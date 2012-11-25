/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class SemanticException : SourceException
	{
		public SemanticException(HappySourceLocation happySourceLocation, string fmt, params object[] args) : base(happySourceLocation, fmt, args)
		{
			
		}
	}
}

