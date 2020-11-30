using System.Text.Json.Serialization;

namespace OwnID.Web.Gigya.Contracts.Jwt
{
    public class GetJwtResponse : BaseGigyaResponse
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }
}