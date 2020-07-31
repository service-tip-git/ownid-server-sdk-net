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

namespace OwnIdSdk.NetCore3.Flow.Commands.Link
{
    public class SaveAccountLinkCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly ILocalizationService _localizationService;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;

        public SaveAccountLinkCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IAccountLinkHandlerAdapter linkHandlerAdapter,
            ILocalizationService localizationService, IOwnIdCoreConfiguration coreConfiguration)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _linkHandlerAdapter = linkHandlerAdapter;
            _localizationService = localizationService;
            _coreConfiguration = coreConfiguration;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForLink)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SaveAccountLinkCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserProfileData>(requestJwt.Data.Jwt).Data;

            // preventing data substitution
            userData.DID = relatedItem.DID;

            if (!_coreConfiguration.OverwriteFields)
                userData.Profile = null;
            
            var formContext = _linkHandlerAdapter.CreateUserDefinedContext(userData, _localizationService);

            if (_coreConfiguration.OverwriteFields)
            {
                formContext.Validate();

                if (formContext.HasErrors)
                    throw new BusinessValidationException(formContext);
            }

            await _linkHandlerAdapter.OnLink(formContext);

            if (formContext.HasErrors)
                throw new BusinessValidationException(formContext);

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, userData.DID);
            var jwt = _jwtComposer.GenerateFinalStepJwt(relatedItem.Context,
                _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType), input.CultureInfo?.Name);

            return new JwtContainer(jwt);
        }
    }
}