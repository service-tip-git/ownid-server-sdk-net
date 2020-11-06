using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class GetSecurityCheckCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;

        public GetSecurityCheckCommand(ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController)
        {
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            var r = new Random();
            if (r.Next(1,3) != 1)
                 throw new InternalLogicException("test exception");
            
            if (relatedItem.HasFinalState)
                throw new CommandValidationException(
                    "Cache item should be not have final state" +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var step = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);
            var pin = await _cacheItemService.SetSecurityCodeAsync(relatedItem.Context);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                Behavior = step,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name
            };

            if (!relatedItem.IsStateless)
            {
                composeInfo.EncToken = relatedItem.EncToken;
                composeInfo.CanBeRecovered = !string.IsNullOrEmpty(relatedItem.RecoveryToken);
            }

            var jwt = _jwtComposer.GeneratePinStepJwt(composeInfo, pin);
            return new JwtContainer(jwt);
        }
    }
}