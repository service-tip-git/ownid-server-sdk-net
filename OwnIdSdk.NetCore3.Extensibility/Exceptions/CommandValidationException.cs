using System;

namespace OwnIdSdk.NetCore3.Extensibility.Exceptions
{
    public class CommandValidationException : Exception
    {
        public CommandValidationException(string message) : base(message)
        {
        }
    }
}