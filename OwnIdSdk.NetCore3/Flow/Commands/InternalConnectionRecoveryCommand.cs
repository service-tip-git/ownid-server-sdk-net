using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands
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
            if (!(input is CommandInput<string>))
                throw new InternalLogicException(
                    $"Incorrect input type for {nameof(InternalConnectionRecoveryCommand)}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var comInput = input as CommandInput<string>;
            var result = await _userHandlerAdapter.GetConnectionRecoveryDataAsync(comInput.Data, true);
            var expectedBehavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);
            var jwt = _jwtComposer.GenerateRecoveryDataJwt(relatedItem.Context, input.ClientDate, expectedBehavior,
                result, input.CultureInfo?.Name);

            return new JwtContainer(jwt);
        }
    }
}