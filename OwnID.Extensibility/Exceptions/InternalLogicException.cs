using System;

namespace OwnID.Extensibility.Exceptions
{
    public class InternalLogicException : Exception
    {
        public InternalLogicException(string message) : base(message)
        {
        }

        public InternalLogicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}