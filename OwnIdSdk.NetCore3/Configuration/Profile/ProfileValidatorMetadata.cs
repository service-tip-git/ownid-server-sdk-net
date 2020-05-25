using System;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    public class ProfileValidationRuleMetadata
    {
        public string Type { get; set; }
        
        public bool NeedsInternalLocalization { get; set; }
        
        public Func<string> GetErrorMessageKey { get; set; }
    }
}