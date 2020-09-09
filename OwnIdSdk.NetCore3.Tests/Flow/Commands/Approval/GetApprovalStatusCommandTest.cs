using System;
using System.Globalization;
using System.Threading.Tasks;
using Moq;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Commands;
using OwnIdSdk.NetCore3.Flow.Commands.Approval;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Tests.TestUtils;
using Xunit;

namespace OwnIdSdk.NetCore3.Tests.Flow.Commands.Approval
{
    public class GetApprovalStatusCommandTest
    {
        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_Decline_Success(Mock<IJwtComposer> jwtComposer,
            Mock<IFlowController> flowController, IAccountRecoveryHandler recoveryHandler,
            IServiceProvider serviceProvider, CacheItem cacheItem, FrontendBehavior frontendBehavior)
        {
            var requestIdentity = new RequestIdentity
            {
                Context = cacheItem.Context,
                RequestToken = cacheItem.RequestToken,
                ResponseToken = cacheItem.ResponseToken
            };

            cacheItem.Status = CacheItemStatus.Declined;
            const StepType currentStepType = StepType.ApprovePin;
            flowController
                .Setup(x => x.GetExpectedFrontendBehavior(It.Is<CacheItem>(y => y.Context == cacheItem.Context),
                    It.Is<StepType>(y => y == currentStepType))).Returns(frontendBehavior);

            var commandInput = new CommandInput(requestIdentity, CultureInfo.CurrentCulture, DateTime.Now);
            var command = new GetApprovalStatusCommand(jwtComposer.Object, flowController.Object, recoveryHandler,
                serviceProvider);
            await command.ExecuteAsync(commandInput, cacheItem, currentStepType);

            flowController.Verify(x => x.GetExpectedFrontendBehavior(cacheItem, currentStepType));
            jwtComposer.Verify(
                x => x.GenerateFinalStepJwt(cacheItem.Context, commandInput.ClientDate, frontendBehavior,
                    commandInput.CultureInfo.Name), Times.Once);
        }
    }
}