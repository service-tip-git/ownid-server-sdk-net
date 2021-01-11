using System.Collections.Generic;
using System.Text.Json.Serialization;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Extensibility.Flow.Contracts.Start
{
    public class StateResult : JwtContainer
    {
        public StateResult(string jwt, List<CookieInfo> cookies) : base(jwt)
        {
            Cookies = cookies;
        }

        [JsonIgnore]
        public List<CookieInfo> Cookies { get; }
    }
}