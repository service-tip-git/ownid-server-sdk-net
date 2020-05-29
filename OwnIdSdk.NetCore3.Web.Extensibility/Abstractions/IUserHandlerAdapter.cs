using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IUserHandlerAdapter
    {
        IFormContext CreateUserDefinedContext(UserProfileData profileData, ILocalizationService localizationService);

        Task UpdateProfileAsync(IFormContext context);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did);
    }
}