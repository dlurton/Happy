using System;
using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class InternalSourceException : SourceException
	{
		public InternalSourceException(HappySourceLocation loc)
			: base(loc) { }
		public InternalSourceException(HappySourceLocation loc, string msg, params object[] args)
			: base(loc, msg, args)  { }

		public InternalSourceException(HappySourceLocation loc, Exception inner, string msg, params object[] args)
			: base(loc, String.Format(msg, args), inner) { }
	}
}