using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    /// <summary>
    ///     POST /ownid/{context}/status request body structure
    /// </summary>
    public class GenerateContextRequest
    {
        /// <summary>
        ///     Lowercased challenge type <see cref="Jwt.ChallengeType" />
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        ///     Flags if qr will be used for current flow
        /// </summary>
        [JsonPropertyName("qr")]
        public bool IsQr { get; set; }

        /// <summary>
        ///     Payload specific for particular implementation
        /// </summary>
        [JsonPropertyName("data")]
        public object Payload { get; set; }
    }
}