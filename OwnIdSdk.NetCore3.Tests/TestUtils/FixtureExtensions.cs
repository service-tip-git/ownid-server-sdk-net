using System.Linq;
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

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            return fixture;
        }
    }
}