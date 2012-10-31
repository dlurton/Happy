using System;

namespace HappyTemplate.Exceptions
{
    public class InternalException : Exception
    {
        public InternalException(string msg) : base (msg) { }

		public InternalException(string msg, params object[] args)
			: base(String.Format(msg, args)) { }

		public InternalException(Exception inner, string msg, params object[] args)
			: base(String.Format(msg, args), inner) { }

    }
}
