using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    /// <summary>
    ///     Account data save at Gigya profile
    /// </summary>
    /// <remarks>
    ///    Being passed as a <code>data</code> parameter during calling accounts.setAccountInfo method
    ///    https://developers.gigya.com/display/GD/accounts.setAccountInfo+REST
    /// </remarks>
    public class AccountData
    {
        /// <summary>
        ///     Contains public keys linked to account
        /// </summary>
        [JsonPropertyName("pubKeys")]
        public List<string> PubKeys { get; set; } = new List<string>(0);

        
        /// <summary>
        ///     Constructor
        /// </summary>
        public AccountData()
        {
        }
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="pubKey">public key</param>
        public AccountData(string pubKey)
        {
            PubKeys = new List<string>(1) {pubKey};
        }
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="pubKeys">public keys</param>
        public AccountData(IEnumerable<string> pubKeys)
        {
            PubKeys = pubKeys.Distinct().ToList();
        }
    }
}