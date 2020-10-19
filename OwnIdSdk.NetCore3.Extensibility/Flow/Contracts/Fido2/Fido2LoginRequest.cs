using System.Text.Json.Serialization;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2LoginRequest : ISignedData
    {
        [JsonPropertyName("fido2")]
        public LoginInfo Info { get; set; }
        
        [JsonPropertyName("pubKey")]
        public string PublicKey { get; set; }
        
        public class LoginInfo
        {
            [JsonPropertyName("credentialId")]
            public string CredentialId { get; set; }
            [JsonPropertyName("authenticatorData")]
            public string AuthenticatorData { get; set; }
            [JsonPropertyName("clientDataJSON")]
            public string ClientDataJSON { get; set; }
            [JsonPropertyName("signature")]
            public string Signature { get; set; }
        }
    }
}