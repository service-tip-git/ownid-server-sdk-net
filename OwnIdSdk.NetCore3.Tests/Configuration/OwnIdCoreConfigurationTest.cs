using FluentAssertions;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Configuration.Profile;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Configuration
{
    public class OwnIdCoreConfigurationTest
    {
        [Fact]
        public void SetProfileModel_Test()
        {
            var config = new OwnIdCoreConfiguration();
            config.SetProfileModel<DefaultProfileModel>();
            config.ProfileConfiguration.ProfileModelType.Should().Be<DefaultProfileModel>();
        }
    }
}