using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Services;

namespace OwnID.Commands.Fido2
{
    public class VerifyFido2CredentialIdCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly CheckUserExistenceCommand _checkUserExistenceCommand;

        public VerifyFido2CredentialIdCommand(ICacheItemRepository cacheItemRepository, CheckUserExistenceCommand checkUserExistenceCommand)
        {
            _cacheItemRepository = cacheItemRepository;
            _checkUserExistenceCommand = checkUserExistenceCommand;
        }

        public async Task ExecuteAsync(CacheItem cacheItem)
        {
            if(string.IsNullOrEmpty(cacheItem.Fido2CredentialId))
                return;

            var userExists = await _checkUserExistenceCommand.ExecuteAsync(new UserIdentification
            {
                AuthenticatorType = ExtAuthenticatorType.Fido2,
                UserIdentifier = cacheItem.Fido2CredentialId
            });

            if (userExists && cacheItem.ChallengeType != ChallengeType.Login)
                throw new OwnIdException(ErrorType.UserAlreadyExists);

            if (!userExists)
                await _cacheItemRepository.UpdateAsync(cacheItem.Context, item =>
                {
                    item.Fido2CredentialId = null;
                });
        }
    }
}