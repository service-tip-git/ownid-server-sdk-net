using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.ConnectionRecovery
{
    public class ConnectionRecoveryResult<TProfile> where TProfile : class
    {
        public TProfile UserProfile { get; set; }

        public string PublicKey { get; set; }

        public string RecoveryData { get; set; }

        [JsonPropertyName("did")]
        public string DID { get; set; }
    }
}