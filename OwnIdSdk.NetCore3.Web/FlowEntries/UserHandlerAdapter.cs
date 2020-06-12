using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class UserHandlerAdapter<TProfile> : IUserHandlerAdapter where TProfile : class
    {
        private readonly IUserHandler<TProfile> _adaptee;

        public UserHandlerAdapter(IUserHandler<TProfile> adaptee)
        {
            _adaptee = adaptee;
        }

        public IFormContext CreateUserDefinedContext(UserProfileData profileData,
            ILocalizationService localizationService)
        {
            return new UserProfileFormContext<TProfile>(profileData.DID, profileData.PublicKey,
                JsonSerializer.Deserialize<TProfile>(profileData.Profile.GetRawText()), localizationService);
        }

        public async Task UpdateProfileAsync(IFormContext context)
        {
            await _adaptee.UpdateProfileAsync(context as UserProfileFormContext<TProfile>);
        }

        public async Task<LoginResult<object>> OnSuccessLoginAsync(string did)
        {
            return await _adaptee.OnSuccessLoginAsync(did);
        }
    }
}