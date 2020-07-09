using System;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Exceptions
{
    public class BusinessValidationException : Exception
    {
        public IFormContext FormContext { get; }

        public BusinessValidationException(IFormContext context)
        {
            FormContext = context;
        }
    }
}