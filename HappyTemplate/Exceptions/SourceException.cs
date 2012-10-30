using System;
using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class SourceException : Exception
	{
		private readonly HappySourceLocation _location;
		readonly Enum _key;

		public HappySourceLocation Location { get { return _location; } }

		public Enum Key { get { return _key; } }


		public SourceException(HappySourceLocation loc)
		{
			_location = loc;
		}

		public SourceException(HappySourceLocation loc, string msg)
			: base(msg)
		{
			_location = loc;
		}

		public SourceException(Exception inner, HappySourceLocation loc, string msg)
			: base(msg, inner)
		{
			_location = loc;
		}

		public SourceException(HappySourceLocation loc, Enum key)
			: this(loc)
		{
			_key = key;
		}

		public SourceException(HappySourceLocation loc, Enum key, string msg)
			: this(loc, msg)
		{
			_key = key;
		}


		public SourceException(Exception inner, HappySourceLocation loc, Enum key, string msg)
			: this(inner, loc, msg)
		{
			_key = key;
		}

		public SourceException(HappySourceLocation loc, string msg, params object[] args)
			: this(loc, Util.Format(msg, args))
		{
		}

		public SourceException(HappySourceLocation loc, Enum key, string msg, params object[] args)
			: this(loc, key, Util.Format(msg, args))
		{
			
		}

		public SourceException(Exception inner, HappySourceLocation loc, Enum key, string msg, params object[] args)
			: this(inner, loc, key, Util.Format(msg, args))
		{

		}

		public override string ToString()
		{
			return String.Format("{0}: {1}", _location, this.Message);
		}

	}
}