using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts.Jwt
{
    public abstract class BaseJwt
    {
        [JsonPropertyName("jti")]
        public string Jti { get; set; }
    }
}