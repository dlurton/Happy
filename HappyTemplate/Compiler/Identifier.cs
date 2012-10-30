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