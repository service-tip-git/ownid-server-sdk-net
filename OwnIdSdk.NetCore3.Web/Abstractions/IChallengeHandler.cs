using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Web.FlowEntries;

namespace OwnIdSdk.NetCore3.Web.Abstractions
{
    public interface IChallengeHandler
    {
        Task UpdateProfileAsync(UserProfileFormContext context);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did, HttpResponse response);
    }
}