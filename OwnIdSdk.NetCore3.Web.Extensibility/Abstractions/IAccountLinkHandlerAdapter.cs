using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IAccountLinkHandlerAdapter
    {
        IFormContext CreateUserDefinedContext(UserProfileData profileData, ILocalizationService localizationService);
        
        Task<string> GetCurrentUserIdAsync(HttpRequest request);

        Task<object> GetUserProfileAsync(string did);

        Task OnLink(IFormContext context);
    }
}