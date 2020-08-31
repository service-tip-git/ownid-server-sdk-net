using System.Threading.Tasks;
using Fido2NetLib;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2RegisterCommand : BaseFido2RegisterCommand
    {
        public Fido2RegisterCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration) : base(fido2, cacheItemService,
            jwtComposer, flowController, configuration)
        {
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            await base.ProcessFido2RegisterResponseAsync(relatedItem, publicKey, signatureCounter, credentialId);

            await CacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, RegisterRequest.UserId, publicKey);
        }
    }
}