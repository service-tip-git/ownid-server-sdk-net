using System;

namespace OwnID.Extensibility.Exceptions
{
    public class CommandValidationException : Exception
    {
        public CommandValidationException(string message) : base(message)
        {
        }
    }
}