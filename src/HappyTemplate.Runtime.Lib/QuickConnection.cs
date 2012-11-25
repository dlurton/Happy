/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections;

namespace HappyTemplate.Runtime.Lib
{
	public class QuickConnection
	{
		readonly string _connectionString;


		public QuickConnection(string connectionString)
		{
			_connectionString = connectionString;
		}

		public QuickCommand CreateCommand(string sql)
		{
			return new QuickCommand(_connectionString, sql);
		}
	}
}

