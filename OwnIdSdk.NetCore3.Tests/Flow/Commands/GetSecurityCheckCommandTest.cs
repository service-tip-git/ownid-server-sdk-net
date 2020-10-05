using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;
using OwnIdSdk.NetCore3.Tests.TestUtils;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Flow.Commands
{
    public class GetSecurityCheckCommandTest
    {
        [Fact]
        public async Task ExecuteAsync_Success()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var cacheItemService = fixture.Create<Mock<ICacheItemService>>();
            var jwtComposer = fixture.Create<Mock<IJwtComposer>>();
            var flowController = fixture.Create<Mock<IFlowController>>();

            var input = fixture.Create<ICommandInput>();
            var relatedItem = new CacheItem
            {
                Context = input.Context,
                ChallengeType = ChallengeType.Link,
                Nonce = fixture.Create<string>(),
                FlowType = FlowType.LinkWithPin,
                RequestToken = input.RequestToken,
                ResponseToken = input.ResponseToken,
                ConcurrentId = fixture.Create<string>(),
                Status = CacheItemStatus.Started,
                DID = fixture.Create<string>()
            };
            const StepType currentStepType = StepType.Starting;
            var expectedString = fixture.Create<string>();

            jwtComposer.Setup(x => x.GeneratePinStepJwt(It.IsAny<BaseJwtComposeInfo>(), It.IsAny<string>())).Returns(
                new Func<BaseJwtComposeInfo, string, string>((c, d) => expectedString));

            var command =
                new GetSecurityCheckCommand(cacheItemService.Object, jwtComposer.Object, flowController.Object);

            var actual = await command.ExecuteAsync(input, relatedItem, currentStepType);

            flowController.Verify(x => x.GetExpectedFrontendBehavior(relatedItem, currentStepType));
            cacheItemService.Verify(x => x.SetSecurityCodeAsync(input.Context), Times.Once);

            var security = await cacheItemService.Object.SetSecurityCodeAsync(input.Context);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = input.Context,
                ClientTime = input.ClientDate,
                Behavior = flowController.Object.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo.Name
            };
            
            //TODO:
//            jwtComposer.Verify(x => x.GeneratePinStepJwt(composeInfo, security));
            actual.Should().BeEquivalentTo(new JwtContainer(expectedString));
        }

        [Fact]
        public async Task ExecuteAsync_Fail_HasFinalState()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var cacheItemService = fixture.Create<Mock<ICacheItemService>>();
            var jwtComposer = fixture.Create<Mock<IJwtComposer>>();
            var flowController = fixture.Create<Mock<IFlowController>>();

            var input = fixture.Create<ICommandInput>();
            var item = fixture.Create<CacheItem>();
            item.RequestToken = input.RequestToken;
            item.ResponseToken = input.ResponseToken;
            item.Status = CacheItemStatus.Finished;
            const StepType currentStepType = StepType.Starting;

            var command =
                new GetSecurityCheckCommand(cacheItemService.Object, jwtComposer.Object, flowController.Object);
            await command.Invoking(x => x.ExecuteAsync(input, item, currentStepType)).Should()
                .ThrowAsync<CommandValidationException>();

            cacheItemService.VerifyNoOtherCalls();
            jwtComposer.VerifyNoOtherCalls();
            flowController.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExecuteAsync_Fail_Tokens()
        {
            var fixture = new Fixture().SetOwnidSpecificSettings();
            var cacheItemService = fixture.Create<Mock<ICacheItemService>>();
            var jwtComposer = fixture.Create<Mock<IJwtComposer>>();
            var flowController = fixture.Create<Mock<IFlowController>>();

            var input = fixture.Create<ICommandInput>();
            var item = fixture.Create<CacheItem>();
            const StepType currentStepType = StepType.Starting;

            var command =
                new GetSecurityCheckCommand(cacheItemService.Object, jwtComposer.Object, flowController.Object);

            await command.Invoking(x => command.ExecuteAsync(input, item, currentStepType)).Should()
                .ThrowAsync<CommandValidationException>();

            cacheItemService.VerifyNoOtherCalls();
            jwtComposer.VerifyNoOtherCalls();
            flowController.VerifyNoOtherCalls();
        }
    }
}