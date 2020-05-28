using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IUserHandlerAdapter
    {
        IFormContext CreateUserDefinedContext(UserProfile profile, ILocalizationService localizationService);

        Task UpdateProfileAsync(IFormContext context);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did);
    }
}