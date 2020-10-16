using System.Text.Json.Serialization;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2RegisterRequest : ISignedData
    {
        [JsonPropertyName("fido2")]
        public RegisterInfo Info { get; set; }

        [JsonPropertyName("pubKey")]
        public string PublicKey { get; set; }
        
        public class RegisterInfo
        {
            [JsonPropertyName("clientDataJSON")]
            public string ClientDataJSON { get; set; }
            [JsonPropertyName("attestationObject")]
            public string AttestationObject { get; set; }
        }
    }
}