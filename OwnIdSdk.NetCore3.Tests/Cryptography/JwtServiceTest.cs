using AutoFixture;
using FluentAssertions;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Tests.TestUtils;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Cryptography
{
    public class JwtServiceTest
    {
        [Fact]
        public void GetJwtHash_Test()
        {
            const string inputStr = "test1";
            const string expectedSha1Hash = "tESsBmE/yNY3lb6a0L6vVQEZNqw=";
            
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var configMock = fixture.Create<OwnIdCoreConfiguration>();
            var jwtService = new JwtService(configMock);
            jwtService.GetJwtHash(inputStr).Should().Be(expectedSha1Hash);
        }
    }
}