using System;

namespace OwnID.Extensibility.Exceptions
{
    public sealed class OwnIdException : Exception
    {
        public ErrorType ErrorType { get; }

        public OwnIdException()
        {
        }

        public OwnIdException(ErrorType errorType, string message) : base(message)
        {
            ErrorType = errorType;
        }

        public OwnIdException(ErrorType errorType, string message, Exception innerException) : base(message,
            innerException)
        {
            ErrorType = errorType;
        }
    }
}