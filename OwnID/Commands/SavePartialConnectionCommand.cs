using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensions;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands
{
    public class SavePartialConnectionCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IUserHandlerAdapter _userHandlerAdapter;
        private readonly ILogger<SavePartialConnectionCommand> _logger;

        public SavePartialConnectionCommand(ICacheItemRepository cacheItemRepository,
            IUserHandlerAdapter userHandlerAdapter, ILogger<SavePartialConnectionCommand> logger)
        {
            _cacheItemRepository = cacheItemRepository;
            _userHandlerAdapter = userHandlerAdapter;
            _logger = logger;
        }

        public async Task ExecuteAsync(UserIdentitiesData input, CacheItem relatedItem)
        {
            var recoveryToken = !string.IsNullOrEmpty(input.RecoveryData) ? Guid.NewGuid().ToString("N") : null;

            var challengeType = relatedItem.ChallengeType;

            // Credentials created at web app
            if (input.ActualChallengeType.HasValue && input.ActualChallengeType != relatedItem.ChallengeType)
            {
                challengeType = input.ActualChallengeType.Value;
            }
            // check if users exists or not. If not - then this is LinkOnLogin
            else if (relatedItem.ChallengeType == ChallengeType.Login)
            {
                var isUserExists = await _userHandlerAdapter.IsUserExistsAsync(input.PublicKey);
                if (!isUserExists)
                    challengeType = ChallengeType.LinkOnLogin;
            }
            
            _logger.LogInformation($"is link on login? {challengeType}");


            await _cacheItemRepository.UpdateAsync(relatedItem.Context, item =>
            {
                item.ChallengeType = challengeType;
                item.RecoveryToken = recoveryToken;
                item.RecoveryData = input.RecoveryData;
                item.FinishFlow(input.DID, input.PublicKey);
            });
        }
    }
}