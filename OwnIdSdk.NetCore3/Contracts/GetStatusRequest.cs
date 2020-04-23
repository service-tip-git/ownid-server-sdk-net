using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    public class GetStatusRequest
    {
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }
    }
}