using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class UnhandledCaseSourceException : InternalSourceException
	{
		public UnhandledCaseSourceException(HappySourceLocation loc)
			: base(loc)
		{
		}

		public UnhandledCaseSourceException(HappySourceLocation loc, string msg)
			: base(loc, msg)
		{
		}
	}
}