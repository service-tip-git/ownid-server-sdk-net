using System.Collections.Generic;
using System.Security.Cryptography;
using OwnIdSdk.NetCore3.Cryptography;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class ProviderConfiguration
    {
        public ProviderConfiguration(string publicKey, string privateKey, string ownIdApplicationUrl, 
            IList<ProfileField> fields, string callbackUrl, Requester requester)
        {
            // add fields check (null, empty)
            
            OwnIdApplicationUrl = ownIdApplicationUrl;
            JwtSignCredentials = RsaHelper.LoadKeys(publicKey, privateKey);
            ProfileFields = fields;
            CallbackUrl = callbackUrl;
            Requester = requester;
        }

        public string OwnIdApplicationUrl { get; }
        
        public RSA JwtSignCredentials { get; }
        
        public IList<ProfileField> ProfileFields { get; }
        
        public string CallbackUrl { get; }
        
        public Requester Requester { get; }
        
        public string Instructions { get; set; }
    }
}