using System;

namespace HappyTemplate.Exceptions
{
    public class InternalException : Exception
    {
        public InternalException(string msg) : base (msg) { }
    }
}
