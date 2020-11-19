using System.Threading.Tasks;
using Fido2NetLib;
using OwnID.Flow.Interfaces;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Providers;

namespace OwnID.Flow.Commands.Fido2
{
    public class Fido2RecoverCommand : BaseFido2RegisterCommand
    {
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public Fido2RecoverCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration,
            IIdentitiesProvider identitiesProvider, IEncodingService encodingService,
            IAccountRecoveryHandler recoveryHandler) : base(fido2, cacheItemService, jwtComposer, flowController,
            configuration, identitiesProvider, encodingService)
        {
            _recoveryHandler = recoveryHandler;
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            var recoverResult = await _recoveryHandler.RecoverAsync(relatedItem.Payload);
            await _recoveryHandler.OnRecoverAsync(recoverResult.DID, new OwnIdConnection
            {
                PublicKey = publicKey,
                Fido2CredentialId = credentialId,
                Fido2SignatureCounter = signatureCounter.ToString()
            });

            await CacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, recoverResult.DID,
                relatedItem.PublicKey);
        }
    }
}