using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     POST /ownid/{context}/status request body structure
    /// </summary>
    public class GenerateContextRequest
    {
        /// <summary>
        ///     Lowercased challenge type <see cref="ChallengeType" />
        /// </summary>
        [JsonPropertyName("type")]
        public ChallengeType Type { get; set; }

        /// <summary>
        ///     Flags if qr will be used for current flow
        /// </summary>
        [JsonPropertyName("qr")]
        public bool IsQr { get; set; }

        /// <summary>
        ///     Partial flor flag
        /// </summary>
        [JsonPropertyName("partial")]
        public bool IsPartial { get; set; }

        /// <summary>
        ///     Payload specific for particular implementation
        /// </summary>
        [JsonPropertyName("data")]
        public object Payload { get; set; }
    }
}