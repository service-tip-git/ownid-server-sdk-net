using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2LinkWithPinCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IAccountLinkHandler _linkHandler;


        public Fido2LinkWithPinCommand(IAccountLinkHandler linkHandler, ICacheItemService cacheItemService,
            IJwtComposer jwtComposer, IFlowController flowController)
        {
            _linkHandler = linkHandler;
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            await _linkHandler.OnLinkAsync(relatedItem.DID, new OwnIdConnection
            {
                PublicKey = relatedItem.PublicKey,
                Fido2CredentialId = relatedItem.Fido2CredentialId,
                Fido2SignatureCounter = relatedItem.Fido2SignatureCounter
            });
            
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name,
                IncludeRequester = true
            };

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, relatedItem.DID,
                relatedItem.PublicKey);
            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, relatedItem.DID);
            return new JwtContainer(jwt);
        }
    }
}