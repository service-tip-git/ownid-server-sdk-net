using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Internal
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