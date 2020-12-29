using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Services;

namespace OwnID.Commands.Recovery
{
    public class RecoverAccountCommand
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;
        private readonly ICacheItemRepository _cacheItemRepository;

        public RecoverAccountCommand(ICacheItemRepository cacheItemRepository,
            IAccountRecoveryHandler accountRecoveryHandler = null)
        {
            _cacheItemRepository = cacheItemRepository;
            _accountRecoveryHandler = accountRecoveryHandler;
        }

        public async Task<CacheItem> ExecuteAsync(CacheItem relatedItem)
        {
            if (_accountRecoveryHandler == null)
                throw new InternalLogicException("Missing Recovery feature");
            
            // Recover access
            var recoverResult = await _accountRecoveryHandler.RecoverAsync(relatedItem.Payload);
            return await _cacheItemRepository.UpdateAsync(relatedItem.Context,
                item => { item.DID = recoverResult.DID; });
        }
    }
}