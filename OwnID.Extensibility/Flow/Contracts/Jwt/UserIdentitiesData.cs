using System.Text.Json.Serialization;
using OwnID.Extensibility.Flow.Contracts.Start;

namespace OwnID.Extensibility.Flow.Contracts.Jwt
{
    /// <summary>
    ///     User partial data container
    /// </summary>
    public class UserIdentitiesData : ISignedData
    {
        /// <summary>
        ///     User unique identifier
        /// </summary>
        [JsonPropertyName("did")]
        public string DID { get; set; }

        /// <summary>
        ///     Encrypted user data for recovery
        /// </summary>
        [JsonPropertyName("recoveryData")]
        public string RecoveryData { get; set; }

        /// <summary>
        ///     User public key generated for current organization / website
        /// </summary>
        /// <remarks>
        ///     Used for JWT validation and user identification
        /// </remarks>
        [JsonPropertyName("pubKey")]
        public string PublicKey { get; set; }
        
        /// <summary>
        ///     Existing connection auth type
        /// </summary>
        [JsonPropertyName("authType")]
        public ConnectionAuthType? AuthType { get; set; }
        
        /// <summary>
        ///     Indicate if user's client supports FIDO2
        /// </summary>
        [JsonPropertyName("supportsFido2")]
        public bool SupportsFido2 { get; set; }
    }
}