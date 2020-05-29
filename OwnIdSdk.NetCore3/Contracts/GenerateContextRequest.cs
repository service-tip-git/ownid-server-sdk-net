using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Contracts
{
    /// <summary>
    ///     POST /ownid/{context}/status request body structure
    /// </summary>
    public class GenerateContextRequest
    {
        /// <summary>
        ///     Lowercased challenge type <see cref="Jwt.ChallengeType" />
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}