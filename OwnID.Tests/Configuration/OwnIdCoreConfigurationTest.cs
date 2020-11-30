using FluentAssertions;
using OwnID.Configuration;
using OwnID.Configuration.Profile;
using Xunit;

namespace OwnID.Tests.Configuration
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