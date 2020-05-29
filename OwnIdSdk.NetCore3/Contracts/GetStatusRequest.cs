using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    /// <summary>
    ///     POST /ownid/{context/status request body structure
    /// </summary>
    public class GetStatusRequest
    {
        /// <summary>
        ///     Nonce
        /// </summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }
    }
}