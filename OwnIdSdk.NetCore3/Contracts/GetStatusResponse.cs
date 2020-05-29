using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    /// <summary>
    ///     POST /ownid/{context}/status default non-success response body structure
    /// </summary>
    public class GetStatusResponse
    {
        [JsonPropertyName("status")] 
        public bool IsSuccess { get; set; }
    }
}