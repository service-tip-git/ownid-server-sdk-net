using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Link
{
    public class SaveAccountLinkCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IAccountLinkHandler _linkHandler;

        public SaveAccountLinkCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IAccountLinkHandler linkHandler)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _linkHandler = linkHandler;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForLink)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SaveAccountLinkCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserIdentitiesData>(requestJwt.Data.Jwt).Data;

            // preventing data substitution
            userData.DID = relatedItem.DID;

            await _linkHandler.OnLinkAsync(userData.DID, new OwnIdConnection
            {
                PublicKey = userData.PublicKey,
                RecoveryToken = relatedItem.RecoveryToken,
                RecoveryData = userData.RecoveryData
            });

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, userData.DID, userData.PublicKey);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name
            };

            var jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}