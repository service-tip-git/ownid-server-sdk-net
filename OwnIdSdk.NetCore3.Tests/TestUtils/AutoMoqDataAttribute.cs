using AutoFixture;
using AutoFixture.Xunit2;

namespace OwnIdSdk.NetCore3.Tests.TestUtils
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().SetOwnidSpecificSettings())
        {
        }
    }
}