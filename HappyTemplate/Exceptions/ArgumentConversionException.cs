using System;

namespace HappyTemplate.Exceptions
{
	public class ArgumentConversionException : ApplicationException
	{
		private readonly int _argumentIndex;

		public ArgumentConversionException(int argumentIndex, Exception inner)
			: base(null, inner)
		{
			_argumentIndex = argumentIndex;
		}

		public int ArgumentIndex
		{
			get { return _argumentIndex; }
		}
	}
}