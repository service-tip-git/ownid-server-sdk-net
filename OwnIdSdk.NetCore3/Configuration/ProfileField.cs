namespace OwnIdSdk.NetCore3.Configuration
{
    public class ProfileField
    {
        public ProfileField(string label, ProfileFieldType fieldType)
        {
            Type = fieldType;
            Label = label;
        }
        
        public ProfileFieldType Type { get; set; }
        
        public string Label { get; set; }
        
        public bool IsRequired { get; set; }
    }
}