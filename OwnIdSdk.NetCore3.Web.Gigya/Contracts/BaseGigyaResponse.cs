using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    public class BaseGigyaResponse
    {
        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }
        
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
       
        [JsonPropertyName("errorDetails")]
        public string ErrorDetails { get; set; }
        
        [JsonPropertyName("callId")]
        public string CallId { get; set; }

        public string GetFailureMessage()
        {
            return $"(CallId={CallId}) {ErrorCode} : {ErrorMessage} ({ErrorDetails})";
        }
    }
}