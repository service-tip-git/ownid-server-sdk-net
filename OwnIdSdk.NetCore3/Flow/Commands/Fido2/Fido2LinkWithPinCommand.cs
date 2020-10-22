using System.Threading.Tasks;
using Fido2NetLib;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2LinkWithPinCommand : BaseFido2RegisterCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IAccountLinkHandler _linkHandler;
        
        public Fido2LinkWithPinCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration, IJwtService jwtService,
            IAccountLinkHandler linkHandler) : base(fido2, cacheItemService, jwtComposer, flowController, configuration,
            jwtService)
        {
            _linkHandler = linkHandler;
            _cacheItemService = cacheItemService;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            await _linkHandler.OnLinkAsync(relatedItem.DID, new OwnIdConnection
            {
                PublicKey = publicKey,
                Fido2CredentialId = credentialId,
                Fido2SignatureCounter = signatureCounter
            });

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, relatedItem.DID, publicKey);
        }
    }
}