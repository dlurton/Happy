/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace Happy.RuntimeLib
{
	public class QuickCommand
	{
		readonly string _conectionString;
		readonly string _sql;
		readonly Dictionary<string, dynamic> _parameters = new Dictionary<string, dynamic>();

		internal QuickCommand(string conectionString, string sql)
		{
			this._conectionString = conectionString;
			_sql = sql;
		}

		public void SetParameter(string name, dynamic value)
		{
			_parameters[name] = value;
		}

		public IEnumerable<dynamic> ExecuteQuery()
		{
			using(var connection = new SqlConnection(_conectionString))
			using(var cmd = connection.CreateCommand())
			{
				cmd.CommandText = _sql;
				foreach (var kvp in _parameters)
					cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);

				connection.Open();
				using(var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
				{
					while(reader.Read())
					{
						IDictionary<string, object> currentRow = new ExpandoObject();
						for (int i = 0; i < reader.FieldCount; ++i)
						{
							currentRow[reader.GetName(i)] = reader.GetValue(i);
						}
						yield return currentRow;
					}
					
				}
			}
		}
	}
}

