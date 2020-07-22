using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts
{
    /// <summary>
    ///     Account data save at Gigya profile
    /// </summary>
    /// <remarks>
    ///     Being passed as a <code>data</code> parameter during calling accounts.setAccountInfo method
    ///     https://developers.gigya.com/display/GD/accounts.setAccountInfo+REST
    /// </remarks>
    public class AccountData
    {
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
            Connections = new List<OwnIdConnection>(1) {new OwnIdConnection {PublicKey = pubKey}};
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="pubKeys">public keys</param>
        public AccountData(IEnumerable<string> pubKeys)
        {
            Connections = pubKeys.Distinct().Select(x => new OwnIdConnection {PublicKey = x}).ToList();
        }

        /// <summary>
        ///     Contains public keys linked to account
        /// </summary>
        [JsonPropertyName("ownIdConnections")]
        public List<OwnIdConnection> Connections { get; set; } = new List<OwnIdConnection>(0);
    }

    public class OwnIdConnection
    {
        [JsonPropertyName("pubKey")] 
        public string PublicKey { get; set; }
        
        // hsh is correct, gigya can not find field hash
        [JsonPropertyName("keyHsh")]
        public string Hash { get; set; }
    }
}