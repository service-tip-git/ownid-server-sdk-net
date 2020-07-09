using System;

namespace OwnIdSdk.NetCore3.Web.FlowEntries.RequestHandling
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RequestDescriptorAttribute : Attribute
    {
        public BaseRequestFields Fields { get; }
        
        public RequestDescriptorAttribute(BaseRequestFields fields = BaseRequestFields.None)
        {
            Fields = fields;
        }
    }
}