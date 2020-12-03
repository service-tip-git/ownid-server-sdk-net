using System.Text.Json.Serialization;
using OwnID.Extensibility.Cache;

namespace OwnID.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     POST /ownid/status response item body structure
    /// </summary>
    public class GetStatusResponse
    {
        /// <summary>
        ///     Status
        /// </summary>
        public CacheItemStatus Status { get; set; }

        /// <summary>
        ///     Context
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        ///     Custom payload
        /// </summary>
        public object Payload { get; set; }
        
        /// <summary>
        ///     Metadata
        /// </summary>
        public string Metadata { get; set; }
    }
}