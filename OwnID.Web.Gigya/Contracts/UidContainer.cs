using System.Text.Json.Serialization;
using OwnID.Web.Gigya.Contracts.Accounts;

namespace OwnID.Web.Gigya.Contracts
{
    public class UidContainer
    {
        [JsonPropertyName("UID")]
        public string UID { get; set; }

        public AccountData Data { get; set; }
    }
}