using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    public class ProfileFieldMetadata
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        
        [JsonPropertyName("key")]
        public string Key { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; }
        
        [JsonPropertyName("validators")]
        public List<ProfileValidationRuleMetadata> Validators { get; set; }
    }
}