using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts
{
    public class UserSettings
    {
        /// <summary>
        ///     Indicates if TFA is enabled for current user
        /// </summary>
        [JsonPropertyName("enforceTfa")]
        public bool? EnforceTFA { get; set; }
    }
}