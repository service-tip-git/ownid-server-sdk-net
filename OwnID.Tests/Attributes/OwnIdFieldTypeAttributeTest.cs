using AutoFixture;
using FluentAssertions;
using OwnID.Attributes;
using OwnID.Extensibility.Configuration.Profile;
using Xunit;

namespace OwnID.Tests.Attributes
{
    public class OwnIdFieldTypeAttributeTest
    {
        [Fact]
        public void Text_Success()
        {
            var attr = new OwnIdFieldTypeAttribute(ProfileFieldType.Text);
            attr.IsValid("sdasd").Should().BeTrue();
        }
        
        [Theory]
        [InlineData("sdsd@as.sd", true)]
        [InlineData("sdsd@as", true)]
        [InlineData("sdsd", false)]
        [InlineData("sdsd@sd@sd.sd", false)]
        public void Email_Test(string email, bool expectedValidationResult)
        {
            var attr = new OwnIdFieldTypeAttribute(ProfileFieldType.Email);
            attr.IsValid(email).Should().Be(expectedValidationResult);
        }

        [Theory]
        [InlineData(ProfileFieldType.Date)]
        [InlineData(ProfileFieldType.Number)]
        [InlineData(ProfileFieldType.Select)]
        [InlineData(ProfileFieldType.Tel)]
        public void NotImplementedTypes_Success(ProfileFieldType type)
        {
            var attr = new OwnIdFieldTypeAttribute(type);
            var fixture = new Fixture();
            attr.IsValid(fixture.Create<string>()).Should().BeTrue();
        }
    }
}