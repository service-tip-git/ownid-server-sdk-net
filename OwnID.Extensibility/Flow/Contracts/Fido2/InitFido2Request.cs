using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts.Fido2
{
    public class InitFido2Request
    {
        [JsonPropertyName("incompatible")]
        public bool IsIncompatible { get; set; }
        
        public string CredId { get; set; }
        
        public string FlowType { get; set; }
    }
}