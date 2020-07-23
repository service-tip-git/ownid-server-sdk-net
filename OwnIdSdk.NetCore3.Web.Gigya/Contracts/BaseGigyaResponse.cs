using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    public class BaseGigyaResponse
    {
        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDetails { get; set; }

        public string CallId { get; set; }

        public string GetFailureMessage()
        {
            return $"(CallId={CallId}) {ErrorCode} : {ErrorMessage} ({ErrorDetails})";
        }
    }
}