using System.Threading.Tasks;
using Fido2NetLib;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Extensions;
using OwnID.Services;

namespace OwnID.Commands.Fido2
{
    public class Fido2LinkCommand : Fido2RegisterCommand
    {
        private readonly IAccountLinkHandler _linkHandler;

        public Fido2LinkCommand(IFido2 fido2, ICacheItemRepository cacheItemRepository,
            IOwnIdCoreConfiguration configuration, IIdentitiesProvider identitiesProvider,
            IEncodingService encodingService, IAccountLinkHandler linkHandler) : base(fido2, cacheItemRepository,
            configuration, identitiesProvider, encodingService)
        {
            _linkHandler = linkHandler;
        }

        protected override async Task<CacheItem> ProcessFido2RegisterResponseAsync(CacheItem relatedItem,
            string publicKey, uint signatureCounter, string credentialId)
        {
            await _linkHandler.OnLinkAsync(relatedItem.DID, new OwnIdConnection
            {
                PublicKey = publicKey,
                Fido2CredentialId = credentialId,
                Fido2SignatureCounter = signatureCounter.ToString(),
                AuthType = ConnectionAuthType.Fido2
            });

            return await CacheItemRepository.UpdateAsync(relatedItem.Context, item =>
            {
                item.FinishFlow(relatedItem.DID, publicKey);
                item.Fido2CredentialId = credentialId;
            });
        }
    }
}