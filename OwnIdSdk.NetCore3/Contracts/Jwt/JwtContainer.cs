using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts.Jwt
{
    public class JwtContainer
    {
        [JsonPropertyName("jwt")]
        public string Jwt { get; set; }
    }
}