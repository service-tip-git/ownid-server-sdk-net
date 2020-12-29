using System;
using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensions;
using OwnID.Services;

namespace OwnID.Commands
{
    public class SavePartialConnectionCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IJwtService _jwtService;

        public SavePartialConnectionCommand(ICacheItemRepository cacheItemRepository, IJwtService jwtService)
        {
            _cacheItemRepository = cacheItemRepository;
            _jwtService = jwtService;
        }

        public async Task ExecuteAsync(JwtContainer input, CacheItem relatedItem)
        {
            var userData = _jwtService.GetDataFromJwt<UserIdentitiesData>(input.Jwt).Data;

            if (string.IsNullOrEmpty(userData.PublicKey))
                throw new CommandValidationException("No public key was provided for partial flow");

            var recoveryToken = !string.IsNullOrEmpty(userData.RecoveryData) ? Guid.NewGuid().ToString("N") : null;

            await _cacheItemRepository.UpdateAsync(relatedItem.Context, item =>
            {
                item.RecoveryToken = recoveryToken;
                item.RecoveryData = userData.RecoveryData;
                item.FinishFlow(userData.DID, userData.PublicKey);
            });
        }
    }
}