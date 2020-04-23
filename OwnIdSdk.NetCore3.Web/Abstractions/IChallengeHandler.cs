using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OwnIdSdk.NetCore3.Web.Abstractions
{
    public interface IChallengeHandler
    {
        Task UpdateProfileAsync(string did, object profile);

        Task OnSuccessLoginAsync(string did, HttpRequest context);
    }
}