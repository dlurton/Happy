/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

namespace Happy.ScriptEngine.Compiler
{
	class Character
	{
		readonly int _index;
		readonly int _lineNo = 1;
		readonly int _columnNo = 1;
		readonly string _fileName;
		readonly char _c;

		public Character(int index, int lineNo, int columNo, string filename, char c)
		{
			_index = index;
			_lineNo = lineNo;
			_columnNo = columNo;
			_fileName = filename;
			_c = c;
		}

		public int Index { get { return _index; } }
		public int LineNo { get { return _lineNo; } }
		public int ColumnNo { get { return _columnNo; } }
		public string FileName { get { return _fileName; } }
		public char C { get { return _c; } }
	}
}

