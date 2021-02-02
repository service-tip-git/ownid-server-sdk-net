using System.Threading.Tasks;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Flow.Adapters;

namespace OwnID.Commands
{
    public class CheckUserExistenceCommand
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public CheckUserExistenceCommand(IUserHandlerAdapter userHandlerAdapter)
        {
            _userHandlerAdapter = userHandlerAdapter;
        }

        public async Task<bool> ExecuteAsync(UserIdentification input)
        {
            // Do nothing for recovery flow
            // var relatedItem = await _cacheItemService.GetCacheItemByContextAsync(input.Context);
            // if (relatedItem.ChallengeType == ChallengeType.Recover)
            //     return false;

            bool result;

            if (string.IsNullOrWhiteSpace(input.UserIdentifier))
                result = false;
            else if (!input.AuthenticatorType.HasValue)
                result = await _userHandlerAdapter.IsUserExistsAsync(input.UserIdentifier);
            else
                result = input.AuthenticatorType switch
                {
                    ExtAuthenticatorType.Fido2 =>
                        await _userHandlerAdapter.IsFido2UserExistsAsync(input.UserIdentifier),
                    _ => false
                };

            return result;
        }
    }
}