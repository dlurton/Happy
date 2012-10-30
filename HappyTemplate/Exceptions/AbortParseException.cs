using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class AbortParseException : SourceException
	{
		public AbortParseException(HappySourceLocation loc)
			: base(loc)
		{

		}
	}
}