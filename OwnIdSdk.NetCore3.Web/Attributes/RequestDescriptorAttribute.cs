using System;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;

namespace OwnIdSdk.NetCore3.Web.Attributes
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