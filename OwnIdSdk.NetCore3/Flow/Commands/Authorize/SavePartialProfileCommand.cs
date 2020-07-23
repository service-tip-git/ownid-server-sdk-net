using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Authorize
{
    public class SavePartialProfileCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;

        public SavePartialProfileCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
        }

        protected override void Validate()
        {
            // TODO 
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!relatedItem.IsValidForAuthorize)
                throw new CommandValidationException(
                    "Cache item should be not be Finished with PARTIAL Login or Register challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType.ToString()}");

            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SavePartialProfileCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserPartialData>(requestJwt.Data.Jwt).Data;

            if (string.IsNullOrEmpty(userData.PublicKey))
                throw new CommandValidationException("No public key was provided for partial flow");

            await _cacheItemService.SetPublicKeyAsync(relatedItem.Context, userData.PublicKey);

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, userData.DID);
            var jwt = _jwtComposer.GenerateFinalStepJwt(relatedItem.Context,
                _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType), input.CultureInfo?.Name);
            return new JwtContainer(jwt);
        }
    }
}