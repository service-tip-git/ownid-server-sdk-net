using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     Approve request structure
    /// </summary>
    public class ApproveActionRequest
    {
        /// <summary>
        ///     Context
        /// </summary>
        [JsonPropertyName("context")]
        public string Context { get; set; }

        /// <summary>
        ///     Nonce
        /// </summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        ///     Indicates if action is approved
        /// </summary>
        [JsonPropertyName("approved")]
        public bool IsApproved { get; set; }
    }
}