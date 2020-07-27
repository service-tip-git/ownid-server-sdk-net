using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Flow.Adapters
{
    public interface IUserHandlerAdapter
    {
        IFormContext CreateUserDefinedContext(UserProfileData profileData, ILocalizationService localizationService);

        Task UpdateProfileAsync(IFormContext context);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did);

        Task<LoginResult<object>> OnSuccessLoginByPublicKeyAsync(string publicKey);
    }
}