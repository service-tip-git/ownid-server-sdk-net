using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    public class BaseGigyaResponse
    {
        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }
        
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}