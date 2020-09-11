using System.Threading.Tasks;
using Fido2NetLib;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2LinkCommand : BaseFido2RegisterCommand
    {
        private readonly IAccountLinkHandler _linkHandler;

        public Fido2LinkCommand(
            IFido2 fido2,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController,
            IOwnIdCoreConfiguration configuration,
            IAccountLinkHandler linkHandler
        ) : base(fido2, cacheItemService, jwtComposer, flowController, configuration)
        {
            _linkHandler = linkHandler;
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            await _linkHandler.OnLinkAsync(
                relatedItem.DID,
                publicKey,
                credentialId,
                signatureCounter
            );

            await CacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, relatedItem.DID, publicKey);
        }
    }
}