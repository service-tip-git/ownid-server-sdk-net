using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Abstractions
{
    public interface IChallengeHandlerAdapter
    {
        IUserProfileContext CreateUserDefinedContext(UserProfile profile, ILocalizationService localizationService);

        Task UpdateProfileAsync(IUserProfileContext context);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did, HttpResponse response);
    }
}