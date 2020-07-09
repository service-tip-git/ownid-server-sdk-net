using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class AccountLinkHandlerAdapter<TProfile> : IAccountLinkHandlerAdapter where TProfile : class
    {
        private readonly IAccountLinkHandler<TProfile> _adaptee;

        public AccountLinkHandlerAdapter(IAccountLinkHandler<TProfile> adaptee)
        {
            _adaptee = adaptee;
        }

        public IFormContext CreateUserDefinedContext(UserProfileData profileData, ILocalizationService localizationService)
        {
            return new UserProfileFormContext<TProfile>(profileData.DID, profileData.PublicKey,
                JsonSerializer.Deserialize<TProfile>(profileData.Profile.GetRawText()), localizationService);
        }

        public async Task<string> GetCurrentUserIdAsync(HttpRequest request)
        {
            return await _adaptee.GetCurrentUserIdAsync(request);
        }

        public async Task<object> GetUserProfileAsync(string did)
        {
            return await _adaptee.GetUserProfileAsync(did);
        }

        public async Task OnLink(IFormContext context)
        {
            await _adaptee.OnLink(context as IUserProfileFormContext<TProfile>);
        }
    }
}