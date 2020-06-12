using System.Text.Json.Serialization;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts.Jwt
{
    public class LinkChallengeData
    {
        [JsonPropertyName("data")]
        public JwtContainer Data { get; set; }
    }
}