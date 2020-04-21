using System.Collections.Generic;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class ProviderConfiguration
    {
        public string OwnIdApplicationUrl { get; set; }
        
        public string TokenSecret { get; set; }
        
        public IList<ProfileField> ProfileFields { get; set; }
        
        public string CallbackUrl { get; set; }
        
        public Requester Requester { get; set; }
    }
}