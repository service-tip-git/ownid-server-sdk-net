using System;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;

namespace OwnIdSdk.NetCore3.Extensibility.Exceptions
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