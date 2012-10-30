namespace SqlGen.Schema
{
	public class ColumnData
	{
		public string Name { get; private set; }
		public string DataType { get; private set; }
		public int? Length { get; private set; }

		public ColumnData(string name, string dataType, int? length)
		{
			this.Name = name;
			this.Length = length;
			this.DataType = dataType;
		}
	}
}