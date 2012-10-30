using HappyTemplate.Compiler;

namespace HappyTemplate.Exceptions
{
	public class SemanticException : SourceException
	{
		public SemanticException(HappySourceLocation happySourceLocation, string fmt, params object[] args) : base(happySourceLocation, fmt, args)
		{
			
		}
	}
}
