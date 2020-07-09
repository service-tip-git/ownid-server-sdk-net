using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts.Jwt
{
    public class GetJwtResponse : BaseGigyaResponse
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }
}