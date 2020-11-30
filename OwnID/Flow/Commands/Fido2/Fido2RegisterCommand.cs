using System.Threading.Tasks;
using Fido2NetLib;
using OwnID.Flow.Interfaces;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Providers;

namespace OwnID.Flow.Commands.Fido2
{
    public class Fido2RegisterCommand : BaseFido2RegisterCommand
    {
        public Fido2RegisterCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration,
            IIdentitiesProvider identitiesProvider, IEncodingService encodingService) : base(fido2, cacheItemService,
            jwtComposer, flowController, configuration, identitiesProvider, encodingService)
        {
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            await base.ProcessFido2RegisterResponseAsync(relatedItem, publicKey, signatureCounter, credentialId);

            await CacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, NewUserId, publicKey);
        }
    }
}