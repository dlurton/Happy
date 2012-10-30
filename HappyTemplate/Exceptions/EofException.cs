using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class EofException : AbortParseException
	{
		public EofException(HappySourceLocation loc)
			: base(loc)
		{

		}
	}
}