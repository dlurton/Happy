namespace HappyTemplate.Compiler
{
	public class Token
	{
		internal readonly HappyTokenKind HappyTokenKind;
		public HappySourceLocation Location { get; private set;  }

		internal readonly string Text;
		internal Token(HappySourceLocation loc, HappyTokenKind happyTokenKind, string text)
		{
			this.HappyTokenKind = happyTokenKind;
			this.Location = loc;
			this.Text = text;
		}

		internal Identifier ToIdentifier()
		{
			return new Identifier(this.Location, this.Text);
		}

		public bool IsOperator
		{
			get
			{
				//TO DO:  is there a better way to do this?
				return this.HappyTokenKind.ToString().StartsWith("Operator");
			}
		}
		public bool IsKeyword
		{
			get
			{
				//TO DO:  is there a better way to do this?
				return this.HappyTokenKind.ToString().StartsWith("Keyword");
			}
		}
	}
}