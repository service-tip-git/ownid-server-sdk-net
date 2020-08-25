using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2LoginRequest
    {
        [JsonPropertyName("fido2")]
        public LoginInfo Info { get; set; }
        
        public class LoginInfo
        {
            public string CredentialId { get; set; }
            public string AuthenticatorData { get; set; }
            public string ClientDataJSON { get; set; }
            public string Signature { get; set; }
            public string UserId { get; set; }
        }
    }
}