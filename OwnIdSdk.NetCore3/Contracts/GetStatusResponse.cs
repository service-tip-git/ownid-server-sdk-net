using System;
using System.Text.Json.Serialization;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Contracts
{
    /// <summary>
    ///     POST /ownid/status response item body structure
    /// </summary>
    public class GetStatusResponse
    {
        /// <summary>
        /// Status
        /// </summary>
        [JsonPropertyName("status")] 
        public CacheItemStatus Status { get; set; }
        /// <summary>
        /// Context
        /// </summary>
        [JsonPropertyName("context")]
        public String Context { get; set; }
        /// <summary>
        /// Custom payload
        /// </summary>
        [JsonPropertyName("payload")]
        public object Payload { get; set; }
    }
}