using System.Collections.Generic;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Internal
{
    public class StateResult
    {
        public StateResult()
        {
            Cookies = new List<CookieInfo>();
        }

        public List<CookieInfo> Cookies { get; set; }
    }
}