using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts.Jwt
{
    public class UserProfile
    {
        [JsonPropertyName("did")]
        public string DID { get; set; }
        
        [JsonPropertyName("pubKey")]
        public string PublicKey { get; set; }
        
        [JsonPropertyName("profile")]
        public Dictionary<string, string> Profile { get; set; }
    }
}