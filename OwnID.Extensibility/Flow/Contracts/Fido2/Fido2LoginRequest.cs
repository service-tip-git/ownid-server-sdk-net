using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts.Fido2
{
    public class Fido2LoginRequest
    {
        public string CredentialId { get; set; }

        public string AuthenticatorData { get; set; }

        [JsonPropertyName("clientDataJSON")]
        public string ClientDataJson { get; set; }

        public string Signature { get; set; }
    }
}