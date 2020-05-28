using System;
using System.IO;
using System.Security.Cryptography;
using OwnIdSdk.NetCore3.Configuration.Profile;
using OwnIdSdk.NetCore3.Cryptography;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class OwnIdCoreConfiguration : IOwnIdCoreConfiguration, IDisposable
    {
        public OwnIdCoreConfiguration() : this(typeof(DefaultProfileModel))
        {
        }

        public OwnIdCoreConfiguration(Type profileType)
        {
            OwnIdApplicationUrl = new Uri(Constants.OwinIdApplicationAddress);
            ProfileConfiguration = new ProfileConfiguration(profileType);
        }

        public Uri OwnIdApplicationUrl { get; set; }

        public Uri CallbackUrl { get; set; }

        public RSA JwtSignCredentials { get; set; }

        public IProfileConfiguration ProfileConfiguration { get; private set; }

        public string DID { get; set; }
        
        public string Name { get; set; }

        public string Icon { get; set; }

        public string Description { get; set; }

        public bool IsDevEnvironment { get; set; }

        // public string RegisterInstructions { get; set; }
        //
        // public string LoginInstructions { get; set; }
        
        public void Dispose()
        {
            JwtSignCredentials?.Dispose();
        }

        public void SetProfileModel<T>() where T : class
        {
            ProfileConfiguration = new ProfileConfiguration(typeof(T));
        }
    }
}