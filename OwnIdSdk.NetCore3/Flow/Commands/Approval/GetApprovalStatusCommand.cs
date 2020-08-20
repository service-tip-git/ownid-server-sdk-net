using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Approval;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Commands.Link;
using OwnIdSdk.NetCore3.Flow.Commands.Recovery;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Approval
{
    public class GetApprovalStatusCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IAccountLinkHandler _linkHandlerAdapter;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public GetApprovalStatusCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IAccountLinkHandler linkHandlerAdapter, IAccountRecoveryHandler recoveryHandler)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _linkHandlerAdapter = linkHandlerAdapter;
            _recoveryHandler = recoveryHandler;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            // TODO:
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            string jwt = null;

            if (relatedItem.Status == CacheItemStatus.Approved)
            {
                BaseFlowCommand command;

                switch (relatedItem.FlowType)
                {
                    case FlowType.LinkWithPin:
                        command = new GetLinkConfigCommand(_jwtComposer, _flowController, _linkHandlerAdapter, false);
                        break;
                    case FlowType.RecoverWithPin:
                        command = new RecoverAccountCommand(_jwtComposer, _flowController, _recoveryHandler, false);
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
                jwt = _jwtComposer.GenerateFinalStepJwt(relatedItem.Context,
                    _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType), input.CultureInfo?.Name
                );
            }

            return new GetApprovalStatusResponse(jwt, relatedItem.Status);
        }
    }
}