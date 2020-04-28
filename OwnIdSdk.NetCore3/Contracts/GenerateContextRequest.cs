using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    public class GenerateContextRequest
    {
        [JsonPropertyName("type")] 
        public string Type { get; set; }
    }
}