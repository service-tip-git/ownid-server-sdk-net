using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts.UpdateProfile
{
    public class GetAccountInfoResponse : BaseGigyaResponse
    {
        [JsonPropertyName("UID")]
        public string DID { get; set; }
        
        [JsonPropertyName("data")]
        public AccountData Data { get; set; }
        
        [JsonPropertyName("profile")]
        public Dictionary<string, string> Profile { get; set; }
    }
}