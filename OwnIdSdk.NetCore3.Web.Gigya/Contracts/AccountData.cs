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
        /// <param name="fido2UserId">FIDO2 user id</param>
        /// <param name="fido2CredentialId">FIDO2 credential id</param>
        /// <param name="fido2SignatureCounter">FIDO2 signature counter</param>
        public AccountData(string pubKey, string fido2UserId = null, string fido2CredentialId = null,
            uint? fido2SignatureCounter = null)
        {
            Connections = new List<OwnIdConnection>(1)
            {
                new OwnIdConnection
                {
                    PublicKey = pubKey,
                    Fido2UserId = fido2UserId,
                    Fido2SignatureCounter = fido2SignatureCounter,
                    Fido2CredentialId = fido2CredentialId
                }
            };
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

        /// <summary>
        ///     Fido2 user id
        /// </summary>
        public string Fido2UserId { get; set; }

        /// <summary>
        ///     Fido2 signature counter
        /// </summary>
        public uint? Fido2SignatureCounter { get; set; }

        /// <summary>
        ///     Fido2 credential id
        /// </summary>
        public string Fido2CredentialId { get; set; }
    }
}