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
