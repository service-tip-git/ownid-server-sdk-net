using System.Threading.Tasks;
using Fido2NetLib;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands.Fido2
{
    public class Fido2UpgradeConnectionCommand : Fido2RegisterCommand
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public Fido2UpgradeConnectionCommand(IFido2 fido2, ICacheItemRepository cacheItemRepository,
            IOwnIdCoreConfiguration configuration, IIdentitiesProvider identitiesProvider,
            IEncodingService encodingService, IUserHandlerAdapter userHandlerAdapter) : base(fido2, cacheItemRepository,
            configuration, identitiesProvider, encodingService)
        {
            _userHandlerAdapter = userHandlerAdapter;
        }

        protected override async Task<CacheItem> ProcessFido2RegisterResponseAsync(CacheItem relatedItem,
            string publicKey, uint signatureCounter, string credentialId)
        {
            await _userHandlerAdapter.UpgradeConnectionAsync(relatedItem.OldPublicKey, new OwnIdConnection
            {
                PublicKey = publicKey,
                Fido2CredentialId = credentialId,
                Fido2SignatureCounter = signatureCounter.ToString(),
                AuthType = ConnectionAuthType.Fido2
            });

            return await base.ProcessFido2RegisterResponseAsync(relatedItem, publicKey, signatureCounter, credentialId);
        }
    }
}