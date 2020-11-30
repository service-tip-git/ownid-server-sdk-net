using System;
using OwnID.Extensibility.Flow.Contracts;

namespace OwnID.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RequestDescriptorAttribute : Attribute
    {
        public RequestDescriptorAttribute(BaseRequestFields fields = BaseRequestFields.None)
        {
            Fields = fields;
        }

        public BaseRequestFields Fields { get; }
    }
}