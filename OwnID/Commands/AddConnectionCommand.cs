using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands
{
    public class AddConnectionCommand
    {
        private readonly IAccountLinkHandler _accountLinkHandler;
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public AddConnectionCommand(IAccountLinkHandler accountLinkHandler, ICacheItemRepository cacheItemRepository,
            IUserHandlerAdapter userHandlerAdapter)
        {
            _accountLinkHandler = accountLinkHandler;
            _cacheItemRepository = cacheItemRepository;
            _userHandlerAdapter = userHandlerAdapter;
        }

        public async Task ExecuteAsync(AddConnectionRequest request)
        {
            var cacheItem = await _cacheItemRepository.GetAsync(request.Context);

            if (cacheItem.Nonce != request.Nonce || cacheItem.Status != CacheItemStatus.Popped)
                throw new CommandValidationException(
                    $"No cacheItem with context {request.Context} and nonce {request.Nonce}");

            if (cacheItem.FlowType != FlowType.Fido2Register && cacheItem.FlowType != FlowType.PartialAuthorize)
                throw new CommandValidationException($"Wrong flow type {cacheItem.FlowType}");

            var connectionExists = !string.IsNullOrEmpty(cacheItem.Fido2CredentialId)
                ? await _userHandlerAdapter.IsFido2UserExistsAsync(cacheItem.Fido2CredentialId)
                : await _userHandlerAdapter.IsUserExistsAsync(cacheItem.PublicKey);

            if (connectionExists)
                throw new CommandValidationException("Can not add connection. Connection already exists");

            await _accountLinkHandler.OnLinkAsync(request.DID, new OwnIdConnection
            {
                Fido2CredentialId = cacheItem.Fido2CredentialId,
                Fido2SignatureCounter = cacheItem.Fido2SignatureCounter.ToString(),
                PublicKey = cacheItem.PublicKey,
                RecoveryToken = cacheItem.RecoveryToken,
                RecoveryData = cacheItem.RecoveryData
            });
        }
    }
}