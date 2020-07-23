using System.Collections.Generic;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    public class BaseGigyaResponse
    {
        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDetails { get; set; }

        public string CallId { get; set; }

        public List<GigyaValidationError> ValidationErrors { get; set; }

        public string GetFailureMessage()
        {
            return $"(CallId={CallId}) {ErrorCode} : {ErrorMessage} ({ErrorDetails})";
        }

        /// <summary>
        ///     Get user friendly failure message (without error code and callId)
        /// </summary>
        /// <returns>User friendly failure message (without error code and callId)</returns>
        public string UserFriendlyFailureMessage => $"{ErrorMessage}: {ErrorDetails}";
    }
}