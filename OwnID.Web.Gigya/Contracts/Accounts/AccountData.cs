using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace OwnID.Web.Gigya.Contracts.Accounts
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
        /// <param name="connection">OwnID Connection Gigya connection data</param>
        public AccountData([NotNull] GigyaOwnIdConnection connection)
        {
            Connections = new List<GigyaOwnIdConnection>(1)
            {
                connection
            };
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="pubKeys">public keys</param>
        public AccountData(IEnumerable<string> pubKeys)
        {
            Connections = pubKeys.Distinct().Select(x => new GigyaOwnIdConnection {PublicKey = x}).ToList();
        }

        /// <summary>
        ///     Contains public keys linked to account
        /// </summary>
        [JsonPropertyName("ownIdConnections")]
        public List<GigyaOwnIdConnection> Connections { get; set; } = new List<GigyaOwnIdConnection>(0);
    }
}