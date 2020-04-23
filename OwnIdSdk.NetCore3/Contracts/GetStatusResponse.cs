using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    public class GetStatusResponse
    {
        [JsonPropertyName("status")]
        public bool IsSuccess { get; set; }
    }
}