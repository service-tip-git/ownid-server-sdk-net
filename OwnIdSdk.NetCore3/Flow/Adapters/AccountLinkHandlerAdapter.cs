using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Flow.Adapters
{
    public class AccountLinkHandlerAdapter<TProfile> : IAccountLinkHandlerAdapter where TProfile : class
    {
        private readonly IAccountLinkHandler<TProfile> _adaptee;
        private readonly JsonSerializerOptions _serializerOptions;

        public AccountLinkHandlerAdapter(IAccountLinkHandler<TProfile> adaptee)
        {
            _adaptee = adaptee;
            _serializerOptions = OwnIdSerializer.GetDefaultProperties();
            _serializerOptions.PropertyNamingPolicy = null;
        }

        public IFormContext CreateUserDefinedContext(UserProfileData profileData,
            ILocalizationService localizationService)
        {
            TProfile profile = default;
            
            if (profileData.Profile.HasValue)
                profile = OwnIdSerializer.Deserialize<TProfile>(profileData.Profile?.GetRawText(), _serializerOptions);
            
            return new UserProfileFormContext<TProfile>(profileData.DID, profileData.PublicKey, profile, localizationService);
        }

        public async Task<string> GetCurrentUserIdAsync(string payload)
        {
            return await _adaptee.GetCurrentUserIdAsync(payload);
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