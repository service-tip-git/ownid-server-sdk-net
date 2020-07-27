using System;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;

namespace OwnIdSdk.NetCore3.Extensibility.Exceptions
{
    public class BusinessValidationException : Exception
    {
        public BusinessValidationException(IFormContext context)
        {
            FormContext = context;
        }

        public IFormContext FormContext { get; }
    }
}