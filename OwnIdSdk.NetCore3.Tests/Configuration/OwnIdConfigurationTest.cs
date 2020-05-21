using System.IO;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Configuration
{
    public class OwnIdConfigurationTest
    {
        [Fact]
        public void SetProfileModel_Test()
        {
            var config = new OwnIdConfiguration();
            config.SetProfileModel<OwnIdConfigurationTest>();
            
            Assert.Equal(typeof(OwnIdConfigurationTest), config.ProfileConfiguration.ProfileModelType);
        }

        [Fact]
        public void SetKeysFromFiles_Test()
        {
            var config = new OwnIdConfiguration();
            config.SetKeysFromFiles("./Keys/jwtRS256.key.pub", "./Keys/jwtRS256.key");
            Assert.NotNull(config.JwtSignCredentials);
            config.Dispose();
        }
    }
}