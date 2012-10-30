using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyTemplate.Exceptions
{
	public class UnhandledCaseException : Exception
	{
		public UnhandledCaseException()
		{

		}
		public UnhandledCaseException(string msg)
			: base(msg)
		{
			
		}
	}
}
