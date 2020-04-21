using System;
using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Web.Abstractions
{
    public interface IChallengeHandler
    {
        Task UpdateProfile(string did, object profile);

        Task<IAsyncResult> OnSuccessLogin(string did);
    }
}