using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts.Internal
{
    public class StateRequest
    {
        public string RecoveryToken { get; set; }
        
        public string EncryptionToken { get; set; }
        
        public bool RequiresRecovery { get; set; }
        
        public string CredId { get; set; }
        
        [JsonIgnore]
        public StateRequest State { get; set; }
    }
}