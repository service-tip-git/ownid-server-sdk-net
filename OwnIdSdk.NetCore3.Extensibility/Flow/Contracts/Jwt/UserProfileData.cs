using System.Text.Json;
using System.Text.Json.Serialization;
using OwnIdSdk.NetCore3.Extensibility.Configuration.Profile;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt
{
    /// <summary>
    ///     Data provided by OwnId application after user enter requested fields with
    ///     <see cref="IProfileConfiguration" />
    /// </summary>
    public class UserProfileData : UserIdentitiesData
    {
        /// <summary>
        ///     Json formatted requested profile fields with
        ///     <see cref="IProfileConfiguration" />
        /// </summary>
        [JsonPropertyName("profile")]
        public JsonElement? Profile { get; set; }
    }
}