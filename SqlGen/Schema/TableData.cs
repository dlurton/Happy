using System.Collections.Generic;

namespace SqlGen.Schema
{
	public class TableData
	{
		private readonly List<ColumnData> _columns = new List<ColumnData>();

		public string Schema { get; private set; }
		public string Name { get; private set; }
		public IEnumerable<ColumnData> Columns { get { return _columns; } }
		public int ColumnCount { get { return _columns.Count; } }

		public TableData(string schema, string name)
		{
			Name = name;
			Schema = schema;
		}

		public void AddColumn(ColumnData columnData)
		{
			_columns.Add(columnData);
		}
	}
}
