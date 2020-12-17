using AutoFixture;
using AutoFixture.Xunit2;

namespace OwnID.Tests.TestUtils
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().SetOwnidSpecificSettings())
        {
        }
    }
}