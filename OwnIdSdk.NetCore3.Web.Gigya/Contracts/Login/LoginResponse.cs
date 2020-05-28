using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts.Login
{
    public class LoginResponse : BaseGigyaResponse
    {
        [JsonPropertyName("sessionInfo")]
        public Dictionary<string, string> SessionInfo { get; set; }
        
        [JsonPropertyName("identities")]
        public IList<Identity> Identities { get; set; }
    }
}