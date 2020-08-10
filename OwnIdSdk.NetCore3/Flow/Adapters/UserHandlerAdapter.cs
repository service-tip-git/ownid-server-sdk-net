using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Flow.Adapters
{
    public class UserHandlerAdapter<TProfile> : IUserHandlerAdapter where TProfile : class
    {
        private readonly IUserHandler<TProfile> _adaptee;
        private readonly JsonSerializerOptions _serializerOptions;

        public UserHandlerAdapter(IUserHandler<TProfile> adaptee)
        {
            _adaptee = adaptee;
            _serializerOptions = OwnIdSerializer.GetDefaultProperties();
            _serializerOptions.PropertyNamingPolicy = null;
        }

        public IFormContext CreateUserDefinedContext(UserProfileData profileData,
            ILocalizationService localizationService)
        {
            return new UserProfileFormContext<TProfile>(profileData.DID, profileData.PublicKey,
                OwnIdSerializer.Deserialize<TProfile>(profileData.Profile?.GetRawText(), _serializerOptions),
                localizationService);
        }

        public async Task CreateProfileAsync(IFormContext context)
        {
            await _adaptee.CreateProfileAsync(context as UserProfileFormContext<TProfile>);
        }

        public async Task UpdateProfileAsync(IFormContext context)
        {
            await _adaptee.UpdateProfileAsync(context as UserProfileFormContext<TProfile>);
        }

        public async Task<IdentitiesCheckResult> CheckUserIdentitiesAsync(string did, string publicKey)
        {
            return await _adaptee.CheckUserIdentitiesAsync(did, publicKey);
        }

        public async Task<LoginResult<object>> OnSuccessLoginAsync(string did)
        {
            return await _adaptee.OnSuccessLoginAsync(did);
        }

        public async Task<LoginResult<object>> OnSuccessLoginByPublicKeyAsync(string publicKey)
        {
            return await _adaptee.OnSuccessLoginByPublicKeyAsync(publicKey);
        }
    }
}