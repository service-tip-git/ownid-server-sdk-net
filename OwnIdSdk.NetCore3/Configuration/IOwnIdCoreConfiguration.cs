using System;
using System.Security.Cryptography;
using OwnIdSdk.NetCore3.Configuration.Profile;

namespace OwnIdSdk.NetCore3.Configuration
{
    public interface IOwnIdCoreConfiguration
    {
        Uri OwnIdApplicationUrl { get; set; }

        Uri CallbackUrl { get; set; }    

        RSA JwtSignCredentials { get; set; }
        
        IProfileConfiguration ProfileConfiguration { get; }

        public string DID { get; set; }
        
        public string Name { get; set; }

        public string Icon { get; set; }

        public string Description { get; set; }

        bool IsDevEnvironment { get; set; }
    }
}