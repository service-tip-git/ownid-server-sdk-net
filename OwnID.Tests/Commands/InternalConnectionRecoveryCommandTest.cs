using System.Threading.Tasks;
using AutoFixture;
using Moq;
using OwnID.Commands;
using OwnID.Flow.Adapters;
using OwnID.Tests.TestUtils;
using Xunit;

namespace OwnID.Tests.Commands
{
    public class InternalConnectionRecoveryCommandTest
    {
        [Fact]
        public async Task Execute_Success()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var userHandler = new Mock<IUserHandlerAdapter>();
            var command = new InternalConnectionRecoveryCommand(userHandler.Object);
            var token = fixture.Create<string>();

            await command.ExecuteAsync(token);
            userHandler.Verify(x=> x.GetConnectionRecoveryDataAsync(token, true), Times.Once);
            userHandler.VerifyNoOtherCalls();
        }
    }
}