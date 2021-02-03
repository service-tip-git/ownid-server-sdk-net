using System;
using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Cookies;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensions;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands
{
    public class LinkAccountCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IJwtService _jwtService;
        private readonly IAccountLinkHandler _linkHandler;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public LinkAccountCommand(ICacheItemRepository cacheItemRepository, IJwtService jwtService,
            IAccountLinkHandler linkHandler, IUserHandlerAdapter userHandlerAdapter)
        {
            _cacheItemRepository = cacheItemRepository;
            _jwtService = jwtService;
            _linkHandler = linkHandler;
            _userHandlerAdapter = userHandlerAdapter;
        }

        public async Task<CacheItem> ExecuteAsync(JwtContainer input, CacheItem relatedItem)
        {
            var userData = _jwtService.GetDataFromJwt<UserIdentitiesData>(input.Jwt).Data;

            if (relatedItem.ChallengeType == ChallengeType.Link && relatedItem.DID != userData.DID)
                throw new CommandValidationException($"Wrong user for linking {userData.DID}");


            var userExists = await _userHandlerAdapter.IsUserExistsAsync(userData.PublicKey);
            if (userExists)
                throw new OwnIdException(ErrorType.UserAlreadyExists);

            // preventing data substitution
            userData.DID = relatedItem.DID;
            
            // TODO: code duplication
            var recoveryToken = !string.IsNullOrEmpty(userData.RecoveryData) ? Guid.NewGuid().ToString("N") : null;

            var connection = new OwnIdConnection
            {
                PublicKey = userData.PublicKey,
                RecoveryToken = recoveryToken,
                RecoveryData = userData.RecoveryData,
                AuthType = relatedItem.AuthCookieType switch
                {
                    CookieType.Fido2 => ConnectionAuthType.Fido2,
                    CookieType.Passcode => ConnectionAuthType.Passcode,
                    _ => ConnectionAuthType.Basic
                }
            };
            
            await _linkHandler.OnLinkAsync(userData.DID, connection);

            return await _cacheItemRepository.UpdateAsync(relatedItem.Context, item =>
            {
                item.RecoveryToken = recoveryToken;
                item.RecoveryData = userData.RecoveryData;
                item.FinishFlow(userData.DID, userData.PublicKey);
            });
        }
    }
}