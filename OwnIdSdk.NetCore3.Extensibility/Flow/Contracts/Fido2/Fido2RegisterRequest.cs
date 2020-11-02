using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2RegisterRequest
    {
        [JsonPropertyName("clientDataJSON")]
        public string ClientDataJson { get; set; }

        public string AttestationObject { get; set; }
    }
}