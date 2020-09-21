using System.Collections.Generic;
using System.Text.Json.Serialization;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts.Accounts;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    public class UidResponse : BaseGigyaResponse
    {
        public List<UidContainer> Results { get; set; }
    }

    public class UidContainer
    {
        [JsonPropertyName("UID")]
        public string UID { get; set; }

        public AccountData Data { get; set; }
    }
}