using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Flow.Adapters
{
    public interface IUserHandlerAdapter
    {
        IFormContext CreateUserDefinedContext(UserProfileData profileData, ILocalizationService localizationService);

        Task CreateProfileAsync(IFormContext context);

        Task UpdateProfileAsync(IFormContext context);

        Task<IdentitiesCheckResult> CheckUserIdentitiesAsync(string did, string publicKey);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did, string publicKey);

        Task<LoginResult<object>> OnSuccessLoginByPublicKeyAsync(string publicKey);

        Task<LoginResult<object>> OnSuccessLoginByFido2Async(string fido2UserId, uint fido2SignCounter);

        Task<Fido2Info> FindFido2Info(string fido2UserId);
    }
}