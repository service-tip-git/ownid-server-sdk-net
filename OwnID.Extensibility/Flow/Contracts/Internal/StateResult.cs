using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts.Internal
{
    public class StateResult
    {
        public StateResult()
        {
            Cookies = new List<CookieInfo>();
        }

        [JsonIgnore]
        public List<CookieInfo> Cookies { get; set; }
    }
}