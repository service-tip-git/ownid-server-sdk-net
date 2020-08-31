using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2LinkWithPinCommand : BaseFlowCommand
    {
        private readonly IAccountLinkHandler _linkHandler;
        private readonly ICacheItemService _cacheItemService;
        private readonly IJwtComposer _jwtComposer;
        private readonly IFlowController _flowController;


        public Fido2LinkWithPinCommand(
            IAccountLinkHandler linkHandler,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController
        )
        {
            _linkHandler = linkHandler;
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            await _linkHandler.OnLinkAsync(
                relatedItem.DID,
                relatedItem.PublicKey,
                relatedItem.Fido2UserId,
                relatedItem.Fido2CredentialId,
                relatedItem.Fido2SignatureCounter
            );

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, 
                relatedItem.DID,
                relatedItem.PublicKey);

            var jwt = _jwtComposer.GenerateBaseStep(
                relatedItem.Context,
                _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                relatedItem.Fido2UserId,
                input.CultureInfo?.Name,
                true);

            return new JwtContainer(jwt);
        }
    }
}