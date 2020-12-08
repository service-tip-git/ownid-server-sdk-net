using System;
using OwnID.Extensibility.Flow.Abstractions;

namespace OwnID.Extensibility.Exceptions
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