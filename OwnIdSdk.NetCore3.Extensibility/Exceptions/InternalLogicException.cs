using System;

namespace OwnIdSdk.NetCore3.Extensibility.Exceptions
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