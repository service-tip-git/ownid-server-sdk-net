using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IUserHandler<T> where T : class
    {
        Task UpdateProfileAsync(IUserProfileFormContext<T> context);

        Task<LoginResult<object>> OnSuccessLoginAsync(string did);
    }
}