using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
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

        public Task<bool> IsUserExists(string publicKey)
        {
            return _adaptee.IsUserExists(publicKey);
        }

        public Task<bool> IsFido2UserExists(string fido2CredentialId)
        {
            return _adaptee.IsFido2UserExists(fido2CredentialId);
        }

        public async Task<AuthResult<object>> OnSuccessLoginAsync(string did, string publicKey)
        {
            return await _adaptee.OnSuccessLoginAsync(did, publicKey);
        }

        public async Task<AuthResult<object>> OnSuccessLoginByPublicKeyAsync(string publicKey)
        {
            return await _adaptee.OnSuccessLoginByPublicKeyAsync(publicKey);
        }

        public Task<AuthResult<object>> OnSuccessLoginByFido2Async(string fido2CredentialId, uint fido2SignCounter)
        {
            return _adaptee.OnSuccessLoginByFido2Async(fido2CredentialId, fido2SignCounter);
        }

        public Task<Fido2Info> FindFido2Info(string fido2CredentialId)
        {
            return _adaptee.FindFido2Info(fido2CredentialId);
        }
    }
}