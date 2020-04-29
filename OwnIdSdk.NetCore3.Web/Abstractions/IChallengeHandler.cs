using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OwnIdSdk.NetCore3.Web.Abstractions
{
    public interface IChallengeHandler
    {
        Task UpdateProfileAsync(string did, Dictionary<string, string> profileFields, string publicKey);

        Task OnSuccessLoginAsync(string did, HttpResponse response);
    }
}