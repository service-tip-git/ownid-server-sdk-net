using System.Threading.Tasks;
using OwnID.Flow.Adapters;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Flow.Commands.Internal
{
    public class InternalConnectionRecoveryCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public InternalConnectionRecoveryCommand(IFlowController flowController, IJwtComposer jwtComposer,
            IUserHandlerAdapter userHandlerAdapter)
        {
            _flowController = flowController;
            _jwtComposer = jwtComposer;
            _userHandlerAdapter = userHandlerAdapter;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var result = await _userHandlerAdapter.GetConnectionRecoveryDataAsync(relatedItem.RecoveryToken, true);
            
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name,
                EncToken = relatedItem.EncToken
            };
            
            var jwt = _jwtComposer.GenerateRecoveryDataJwt(composeInfo, result);
            return new JwtContainer(jwt);
        }
    }
}