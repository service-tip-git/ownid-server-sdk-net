using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class UserHandlerAdapter<T> : IUserHandlerAdapter where T : class
    {
        private readonly IUserHandler<T> _adaptee;

        public UserHandlerAdapter(IUserHandler<T> adaptee)
        {
            _adaptee = adaptee;
        }

        public IFormContext CreateUserDefinedContext(UserProfile profile,
            ILocalizationService localizationService)
        {
            return new UserProfileFormContext<T>(profile.DID, profile.PublicKey,
                JsonSerializer.Deserialize<T>(profile.Profile.GetRawText()), localizationService);
        }

        public async Task UpdateProfileAsync(IFormContext context)
        {
            await _adaptee.UpdateProfileAsync(context as UserProfileFormContext<T>);
        }

        public async Task<LoginResult<object>> OnSuccessLoginAsync(string did)
        {
            return await _adaptee.OnSuccessLoginAsync(did);
        }
    }
}