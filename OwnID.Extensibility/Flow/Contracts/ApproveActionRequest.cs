using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     Approve request structure
    /// </summary>
    public class ApproveActionRequest
    {
        /// <summary>
        ///     Context
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        ///     Nonce
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        ///     Indicates if action is approved
        /// </summary>
        [JsonPropertyName("approved")]
        public bool IsApproved { get; set; }
    }
}