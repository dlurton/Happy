using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HappyTemplate.Exceptions
{
	class HappyRuntimeException : Exception
	{
		public HappyRuntimeException(string msg) : base(msg)
		{
			
		}
	}
}
