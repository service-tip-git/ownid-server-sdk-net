using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    public class UidResponse : BaseGigyaResponse
    {
        public List<UidContainer> Results { get; set; }
    }

    public class UidContainer
    {
        public string UID { get; set; }

        public AccountData Data { get; set; }
    }
}