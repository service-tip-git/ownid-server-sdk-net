using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    /// <summary>
    ///     POST /ownid/status request item structure
    /// </summary>
    public class GetStatusRequest
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
    }
}