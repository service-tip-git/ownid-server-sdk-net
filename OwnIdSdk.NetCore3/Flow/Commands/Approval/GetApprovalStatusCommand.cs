using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Approval;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Commands.Fido2;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Approval
{
    public class GetApprovalStatusCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IAccountRecoveryHandler _recoveryHandler;
        private readonly IServiceProvider _serviceProvider;

        public GetApprovalStatusCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IAccountRecoveryHandler recoveryHandler, IServiceProvider serviceProvider)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _recoveryHandler = recoveryHandler;
            _serviceProvider = serviceProvider;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            string jwt = null;

            if (relatedItem.Status == CacheItemStatus.Approved)
            {
                BaseFlowCommand command;

                switch (relatedItem.FlowType)
                {
                    case FlowType.LinkWithPin:
                        command = new GetNextStepCommand(_jwtComposer, _flowController, false);
                        break;
                    case FlowType.RecoverWithPin:
                        command = new RecoverAccountCommand(_jwtComposer, _flowController, _recoveryHandler, false);
                        break;
                    case FlowType.Fido2LinkWithPin:
                        command = _serviceProvider.GetService<Fido2LinkWithPinCommand>();
                        break;
                    case FlowType.Fido2RecoverWithPin:
                        command = _serviceProvider.GetService<Fido2RecoverWithPinCommand>();
                        break;
                    default:
                        throw new InternalLogicException(
                            $"Not supported FlowType for get approval status '{relatedItem.FlowType.ToString()}'");
                }

                var commandResult = await command.ExecuteAsync(input, relatedItem, currentStepType);

                if (!(commandResult is JwtContainer jwtContainer))
                    throw new InternalLogicException("Incorrect command result type");

                jwt = jwtContainer.Jwt;
            }
            else if (relatedItem.Status == CacheItemStatus.Declined)
            {
                var step = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);
                
                var composeInfo = new BaseJwtComposeInfo
                {
                    Context = relatedItem.Context,
                    ClientTime = input.ClientDate,
                    Behavior = step,
                    Locale = input.CultureInfo?.Name
                };
                
                jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            }

            return new GetApprovalStatusResponse(jwt, relatedItem.Status);
        }
    }
}