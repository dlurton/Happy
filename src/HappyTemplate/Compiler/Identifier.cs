/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace HappyTemplate.Compiler
{
	class Identifier
	{
		readonly HappySourceLocation _location;
		readonly string _text;

		public HappySourceLocation Location { get { return _location; } }
		public string Text { get { return _text; } }


		public Identifier(HappySourceLocation location, string text)
		{
			_location = location;
			_text = text;
		}

		public override string ToString()
		{
			return _text;
		}
	}
}

