using System.Threading.Tasks;
using OwnID.Flow.Commands.Recovery;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts.Approval;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Flow.Commands.Approval
{
    public class GetApprovalStatusCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public GetApprovalStatusCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IAccountRecoveryHandler recoveryHandler)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _recoveryHandler = recoveryHandler;
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
                    case FlowType.Fido2RecoverWithPin:
                        command = new GetNextStepCommand(_jwtComposer, _flowController, false);
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