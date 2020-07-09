using System;

namespace OwnIdSdk.NetCore3.Web.FlowEntries.RequestHandling
{
    [Flags]
    public enum BaseRequestFields
    {
        None = 0,
        Context = 1,
        RequestToken = 2,
        ResponseToken = 4
    }
}