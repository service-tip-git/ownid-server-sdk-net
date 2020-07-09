using System;

namespace OwnIdSdk.NetCore3.Web.Exceptions
{
    public class RequestValidationException : Exception
    {
        public RequestValidationException(string message) : base(message)
        {
        }
    }
}