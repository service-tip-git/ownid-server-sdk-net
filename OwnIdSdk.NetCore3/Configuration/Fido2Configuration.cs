using System;
using OwnIdSdk.NetCore3.Extensibility.Configuration;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class Fido2Configuration : IFido2Configuration
    {
        public bool Enabled { get; set; }
        
        public Uri Origin { get; set; }
        
        public Uri PasswordlessPageUrl { get; set; }
        
        public string RelyingPartyId { get; set; }
        
        public string RelyingPartyName { get; set; }
        
        public string UserDisplayName { get; set; }
        
        public string UserName { get; set; }
    }
}