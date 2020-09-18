using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Authorize
{
    public class SaveProfileCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly ILocalizationService _localizationService;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public SaveProfileCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IUserHandlerAdapter userHandlerAdapter,
            ILocalizationService localizationService, IOwnIdCoreConfiguration coreConfiguration)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _userHandlerAdapter = userHandlerAdapter;
            _localizationService = localizationService;
            _coreConfiguration = coreConfiguration;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForAuthorize)
                throw new CommandValidationException(
                    "Cache item should be not be Finished with Login or Register challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SaveProfileCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserProfileData>(requestJwt.Data.Jwt).Data;

            var checkResult = await _userHandlerAdapter.CheckUserIdentitiesAsync(userData.DID, userData.PublicKey);

            if (checkResult == IdentitiesCheckResult.WrongPublicKey)
                throw new CommandValidationException("Wrong public key");

            if (checkResult == IdentitiesCheckResult.UserNotFound || _coreConfiguration.OverwriteFields)
            {
                if (!userData.Profile.HasValue || userData.Profile.Value.ValueKind != JsonValueKind.Object)
                    throw new CommandValidationException("Profile should be provided for user");

                var formContext = _userHandlerAdapter.CreateUserDefinedContext(userData, _localizationService);

                formContext.Validate();

                if (formContext.HasErrors)
                    throw new BusinessValidationException(formContext);

                switch (checkResult)
                {
                    case IdentitiesCheckResult.UserNotFound:
                        await _userHandlerAdapter.CreateProfileAsync(formContext);
                        break;
                    case IdentitiesCheckResult.UserExists:
                        await _userHandlerAdapter.UpdateProfileAsync(formContext);
                        break;
                }

                if (formContext.HasErrors)
                    throw new BusinessValidationException(formContext);
            }

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, userData.DID, userData.PublicKey);
            var jwt = _jwtComposer.GenerateFinalStepJwt(relatedItem.Context,
                input.ClientDate, _flowController.GetExpectedFrontendBehavior(relatedItem, StepType.Authorize),
                input.CultureInfo?.Name);

            return new JwtContainer(jwt);
        }
    }
}