using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OwnID.Commands;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow.Adapters;
using OwnID.Tests.TestUtils;
using Xunit;

namespace OwnID.Tests.Commands
{
    public class CheckUserExistenceCommandTest
    {
        [Fact]
        public async Task Execute_Null_UserIdentifier()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var userHandler = fixture.Create<Mock<IUserHandlerAdapter>>();
            var command = new CheckUserExistenceCommand(userHandler.Object);

            var result = await command.ExecuteAsync(new UserIdentification());
            userHandler.VerifyNoOtherCalls();
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Execute_Null_AuthenticatorType()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var userHandler = fixture.Create<Mock<IUserHandlerAdapter>>();
            var command = new CheckUserExistenceCommand(userHandler.Object);
            var userId = fixture.Create<string>();
            userHandler.Setup(x => x.IsUserExistsAsync(It.Is<string>(v => userId == v)))
                .Returns(Task.FromResult(true));

            var result = await command.ExecuteAsync(new UserIdentification
            {
                UserIdentifier = userId
            });

            result.Should().Be(true);
        }

        [Theory]
        [InlineData(ExtAuthenticatorType.Fido2, true)]
        [InlineData(ExtAuthenticatorType.Unknown, false)]
        public async Task Execute_AuthenticatorType(ExtAuthenticatorType type, bool expectedResult)
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var userHandler = fixture.Create<Mock<IUserHandlerAdapter>>();
            var command = new CheckUserExistenceCommand(userHandler.Object);
            var userId = fixture.Create<string>();
            userHandler.Setup(x => x.IsFido2UserExistsAsync(It.Is<string>(v => userId == v)))
                .Returns(Task.FromResult(true));

            var result = await command.ExecuteAsync(new UserIdentification
            {
                UserIdentifier = userId,
                AuthenticatorType = type
            });

            result.Should().Be(expectedResult);
        }
    }
}