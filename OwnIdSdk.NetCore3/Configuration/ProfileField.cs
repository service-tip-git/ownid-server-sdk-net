using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class ProfileField
    {
        // For options binding
        public ProfileField()
        {
            Type = ProfileFieldType.Text;
        }

        public ProfileField(string label, string key, bool isRequired = true,
            ProfileFieldType fieldType = ProfileFieldType.Text, string placeholder = null)
        {
            Key = key;
            Label = label;
            IsRequired = isRequired;
            Type = fieldType;
            Placeholder = placeholder;
        }

        [JsonPropertyName("type")] 
        public ProfileFieldType Type { get; set; }

        [JsonPropertyName("label")] 
        public string Label { get; set; }

        [JsonPropertyName("key")] 
        public string Key { get; set; }

        [JsonPropertyName("placeholder")] 
        public string Placeholder { get; set; }

        [JsonPropertyName("required")] 
        public bool IsRequired { get; }

        public static ProfileField Email =>
            new ProfileField("Email", "email", true, ProfileFieldType.Email, "john.doe@mail.com");

        public static ProfileField FirstName =>
            new ProfileField("First Name", "firstName", true, ProfileFieldType.Text, "First name");

        public static ProfileField LastName =>
            new ProfileField("Last Name", "lastName", true, ProfileFieldType.Text, "Last name");
    }
}