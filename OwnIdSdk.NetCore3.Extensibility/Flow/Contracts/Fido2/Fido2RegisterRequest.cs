using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2RegisterRequest
    {
        [JsonPropertyName("fido2")]
        public RegisterInfo Info { get; set; }
        
        public class RegisterInfo
        {
            public string UserId { get; set; }
            public string ClientDataJSON { get; set; }
            public string AttestationObject { get; set; }
        }
    }
}