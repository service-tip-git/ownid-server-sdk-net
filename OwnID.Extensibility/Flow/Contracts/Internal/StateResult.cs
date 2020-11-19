using System.Collections.Generic;

namespace OwnID.Extensibility.Flow.Contracts.Internal
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