using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Configuration.Profile
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
        public string Label { get; set; }

        /// <summary>
        ///     Profile Field html form key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Input type. Is lowercased <see cref="ProfileFieldType" /> value
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        ///     Input placeholder
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        ///     List of validators to set on OwinId app side
        /// </summary>
        public List<ProfileValidationRuleMetadata> Validators { get; set; }
    }
}