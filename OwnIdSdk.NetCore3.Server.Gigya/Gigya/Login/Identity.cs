using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Server.Gigya.Gigya.Login
{
    public class Identity
    {
        [JsonPropertyName("provider")]
        public string Provider { get; set; }
        
        [JsonPropertyName("providerUID")]
        public string ProviderUID { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("nickname")]
        public string NickName { get; set; }
        
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        
        [JsonPropertyName("lastUpdated")]
        public string LastUpdate { get; set; }
        
        [JsonPropertyName("allowsLogin")]
        public bool AllowsLogin { get; set; }
        
        [JsonPropertyName("isLoginIdentity")]
        public bool IsLogIdentity { get; set; }
        
        [JsonPropertyName("isExpiredSession")]
        public bool IsExpiredSession { get; set; }
    }
}