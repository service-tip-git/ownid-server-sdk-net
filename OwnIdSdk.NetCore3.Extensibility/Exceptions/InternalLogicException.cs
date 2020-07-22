using System;

namespace OwnIdSdk.NetCore3.Extensibility.Exceptions
{
    public class InternalLogicException : Exception
    {
        public InternalLogicException(string message) : base(message)
        {
        }
    }
}