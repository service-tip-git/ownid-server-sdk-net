using System;
using System.IO;
using System.Security.Cryptography;
using OwnIdSdk.NetCore3.Configuration.Profile;
using OwnIdSdk.NetCore3.Cryptography;

namespace OwnIdSdk.NetCore3.Configuration
{
    public class OwnIdConfiguration : IDisposable
    {
        public OwnIdConfiguration()
        {
            OwnIdApplicationUrl = new Uri("https://ownid.com/sign");
            Requester = new Requester();
        }

        public Uri OwnIdApplicationUrl { get; set; }

        public Uri CallbackUrl { get; set; }

        public RSA JwtSignCredentials { get; set; }

        public IProfileConfiguration ProfileConfiguration { get; private set; }

        public Requester Requester { get; }

        public bool IsDevEnvironment { get; set; }

        // public string RegisterInstructions { get; set; }
        //
        // public string LoginInstructions { get; set; }
        public void Dispose()
        {
            JwtSignCredentials?.Dispose();
        }

        public void SetKeysFromFiles(string pathToPublicKey, string pathToPrivateKey)
        {
            using var publicKeyReader = File.OpenText(pathToPublicKey);
            using var privateKeyReader = File.OpenText(pathToPrivateKey);
            JwtSignCredentials = RsaHelper.LoadKeys(publicKeyReader, privateKeyReader);
        }

        public void SetProfileModel<T>() where T : class
        {
            ProfileConfiguration = new ProfileConfiguration(typeof(T));
        }
    }
}