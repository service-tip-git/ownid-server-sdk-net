using System;
using System.IO;
using System.Security.Cryptography;
using OwnIdSdk.NetCore3.Configuration.Abstractions;
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

        /// <summary>
        ///     Set JWT sign RSA keys
        /// </summary>
        /// <param name="publicKey">Without placeholders</param>
        /// <param name="privateKey">Without placeholders</param>
        /// <returns></returns>
        public void SetKeysFromBase64(string publicKey, string privateKey)
        {
            JwtSignCredentials = RsaHelper.LoadKeys(publicKey, privateKey);
        }

        public void SetKeysFromFiles(string pathToPublicKey, string pathToPrivateKey)
        {
            using var publicKeyReader = File.OpenText(pathToPublicKey);
            using var privateKeyReader = File.OpenText(pathToPrivateKey);
            SetKeysFromBase64(RsaHelper.ReadKeyFromPem(publicKeyReader), RsaHelper.ReadKeyFromPem(privateKeyReader));
        }

        public void SetProfileModel<T>() where T : class
        {
            ProfileConfiguration = new ProfileConfiguration(typeof(T));
        }
    }
}