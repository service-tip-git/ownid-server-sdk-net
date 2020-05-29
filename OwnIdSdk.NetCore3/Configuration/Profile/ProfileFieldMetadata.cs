using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    /// <summary>
    ///     Stores Profile Field data for OwnId application transferring
    /// </summary>
    /// <remarks>
    ///     All locale viable data stores by its localization key
    /// </remarks>
    public class ProfileFieldMetadata
    {
        /// <summary>
        ///     Label text without localization
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }

        /// <summary>
        ///     Profile Field html form key
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        ///     Input type. Is lowercased <see cref="ProfileFieldType" /> value
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        ///     Input placeholder
        /// </summary>
        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; }

        /// <summary>
        ///     List of validators to set on OwinId app side
        /// </summary>
        [JsonPropertyName("validators")]
        public List<ProfileValidationRuleMetadata> Validators { get; set; }
    }
}