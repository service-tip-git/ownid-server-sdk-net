using System.Threading.Tasks;
using Fido2NetLib;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2RecoverCommand : BaseFido2RegisterCommand
    {
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public Fido2RecoverCommand(
            IFido2 fido2,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController,
            IOwnIdCoreConfiguration configuration,
            IAccountRecoveryHandler recoveryHandler
        ) : base(fido2, cacheItemService, jwtComposer, flowController, configuration)
        {
            _recoveryHandler = recoveryHandler;
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter,
            string credentialId)
        {
            var recoverResult = await _recoveryHandler.RecoverAsync(relatedItem.Payload);
            await _recoveryHandler.OnRecoverAsync(
                recoverResult.DID,
                publicKey,
                RegisterRequest.UserId,
                credentialId,
                signatureCounter);

            await CacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context,
                recoverResult.DID,
                relatedItem.PublicKey);
        }
    }
}