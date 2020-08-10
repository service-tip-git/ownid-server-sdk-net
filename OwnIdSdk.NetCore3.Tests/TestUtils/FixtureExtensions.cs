using System.Security.Cryptography;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace OwnIdSdk.NetCore3.Tests.TestUtils
{
    public static class FixtureExtensions
    {
        public static Fixture SetOwnidSpecificSettings(this Fixture fixture)
        {
            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true
            });

            fixture.Register<RSA>(() => null);

            return fixture;
        }
    }
}