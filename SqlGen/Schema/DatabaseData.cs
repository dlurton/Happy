using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlGen.Schema
{
	public class DatabaseData : IDisposable
	{
		string schemaQuery =
			@"SELECT 
	s.name AS SchemaName,
	t.name AS TableName,
	c.name AS ColumnName,
	UPPER(ty.name) AS DataType,
	c.max_length AS Length,
	c.precision AS Precision,
	c.is_nullable AS IsNullable,
	c.is_identity AS IsIdentity,
	c.is_computed AS IsComputed,
	CASE WHEN c.column_id IN 
	(
		SELECT
			innerc.column_id
		FROM sys.tables t
			JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
			JOIN sys.index_columns ic ON i.index_id = ic.index_id AND t.object_id = ic.object_id
			JOIN sys.columns innerc ON t.object_id = innerc.object_id AND ic.column_id = innerc.column_id 
		WHERE
			innerc.object_id = t.object_id
	) THEN 1 ELSE 0 END AS IsPrimaryKey
FROM sys.tables t
	JOIN sys.schemas s ON t.schema_id = s.schema_id
	JOIN sys.columns c ON c.object_id = t.object_id
	JOIN sys.types ty ON c.user_type_id = ty.user_type_id
ORDER BY t.name, c.column_id";

		public string ServerName { get; private set; }
		public string Catalog { get; private set; }
		public IEnumerable<TableData> Tables
		{
			get
			{
				if(_tables != null)
				{
					foreach(var t in _tables)
						yield return t;

					yield break;
				}

				_tables = new List<TableData>();

				openConnection();
				var cmd = _connection.CreateCommand();
				cmd.CommandText = schemaQuery;
				var reader = cmd.ExecuteReader(CommandBehavior.SingleResult);

				if(reader.HasRows)
				{
					reader.Read();
					var table = getTableSchema(reader);
					_tables.Add(table);
					var currentTableName = getTableName(reader);
					while(reader.Read())
					{
						table.AddColumn(getColumnSchema(reader));
						
						var nextTableName = getTableName(reader);
						if(currentTableName != nextTableName)
						{
							yield return table;
							table = getTableSchema(reader);
							_tables.Add(table);
							currentTableName = nextTableName;
						}
					}
					yield return table;
				}

			}
		}

		static ColumnData getColumnSchema(SqlDataReader reader)
		{
			return new ColumnData(reader["ColumnName"].ToString(), reader["DataType"].ToString(), Convert.ToInt32(reader["Length"]));
		}

		static TableData getTableSchema(SqlDataReader reader)
		{
			return new TableData(reader["SchemaName"].ToString(), reader["TableName"].ToString());
		}

		static string getTableName(SqlDataReader reader)
		{
			return getTableSchema(reader) + "." + reader["TableName"];
		}

		void openConnection()
		{
			if (_connection == null)
			{
				var stringBuilder = new SqlConnectionStringBuilder
				{
					DataSource = this.ServerName, 
					InitialCatalog = this.Catalog, 
					IntegratedSecurity = true
				};
				_connection = new SqlConnection(stringBuilder.ConnectionString);
				_connection.Open();
			}
		}


		SqlConnection _connection;
		List<TableData> _tables;

		public DatabaseData(string serverName, string catalog)
		{
			Catalog = catalog;
			ServerName = serverName;
		}


		public void Dispose()
		{
			if (_connection != null)
				_connection.Dispose();
		}
	}
}