using System.Text.Json;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts.Jwt
{
    /// <summary>
    ///     Data provided by OwnId application after user enter requested fields with
    ///     <see cref="OwnIdSdk.NetCore3.Configuration.Profile.IProfileConfiguration" />
    /// </summary>
    public class UserProfileData
    {
        /// <summary>
        ///     User unique identifier
        /// </summary>
        [JsonPropertyName("did")]
        public string DID { get; set; }

        /// <summary>
        ///     User public key generated for current organization / website
        /// </summary>
        /// <remarks>
        ///     Used for JWT validation and user identification
        /// </remarks>
        [JsonPropertyName("pubKey")]
        public string PublicKey { get; set; }

        /// <summary>
        ///     Json formatted requested profile fields with
        ///     <see cref="OwnIdSdk.NetCore3.Configuration.Profile.IProfileConfiguration" />
        /// </summary>
        [JsonPropertyName("profile")]
        public JsonElement Profile { get; set; }
    }
}