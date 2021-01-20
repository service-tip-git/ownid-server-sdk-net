using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
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
        private readonly IOwnIdCoreConfiguration _coreConfiguration;

        public AddConnectionCommand(IAccountLinkHandler accountLinkHandler, ICacheItemRepository cacheItemRepository,
            IUserHandlerAdapter userHandlerAdapter, IOwnIdCoreConfiguration coreConfiguration)
        {
            _accountLinkHandler = accountLinkHandler;
            _cacheItemRepository = cacheItemRepository;
            _userHandlerAdapter = userHandlerAdapter;
            _coreConfiguration = coreConfiguration;
        }

        public async Task<AuthResult> ExecuteAsync(AddConnectionRequest request)
        {
            var cacheItem = await _cacheItemRepository.GetAsync(request.Context);

            if (cacheItem.Nonce != request.Nonce || cacheItem.Status != CacheItemStatus.Popped)
                throw new CommandValidationException(
                    $"No cacheItem with context {request.Context} and nonce {request.Nonce}");

            if (cacheItem.FlowType != FlowType.Fido2Register && cacheItem.FlowType != FlowType.PartialAuthorize)
                throw new CommandValidationException($"Wrong flow type {cacheItem.FlowType}");

            var connectionState = await _accountLinkHandler.GetCurrentUserLinkStateAsync(request.Payload);

            if (connectionState.ConnectedDevicesCount >= _coreConfiguration.MaximumNumberOfConnectedDevices)
                return new AuthResult(
                    $"Maximum number ({_coreConfiguration.MaximumNumberOfConnectedDevices}) of linked devices reached");
            
            var connectionExists = !string.IsNullOrEmpty(cacheItem.Fido2CredentialId)
                ? await _userHandlerAdapter.IsFido2UserExistsAsync(cacheItem.Fido2CredentialId)
                : await _userHandlerAdapter.IsUserExistsAsync(cacheItem.PublicKey);

            if (connectionExists)
                return new AuthResult("Can not add connection. Connection already exists");

            await _accountLinkHandler.OnLinkAsync(connectionState.DID, new OwnIdConnection
            {
                Fido2CredentialId = cacheItem.Fido2CredentialId,
                Fido2SignatureCounter = cacheItem.Fido2SignatureCounter.ToString(),
                PublicKey = cacheItem.PublicKey,
                RecoveryToken = cacheItem.RecoveryToken,
                RecoveryData = cacheItem.RecoveryData
            });

            return new AuthResult();
        }
    }
}