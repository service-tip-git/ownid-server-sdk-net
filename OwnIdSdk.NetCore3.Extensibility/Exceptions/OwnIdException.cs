using System;

namespace OwnIdSdk.NetCore3.Extensibility.Exceptions
{
    public class OwnIdException : Exception
    {
        public OwnIdException()
        {
        }

        public OwnIdException(string message) : base(message)
        {
        }

        public OwnIdException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}