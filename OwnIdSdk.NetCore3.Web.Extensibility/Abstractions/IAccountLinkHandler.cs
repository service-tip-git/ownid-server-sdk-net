using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    //TODO: add docs
    /// <summary>
    ///     Describes base Account link handling operations
    /// </summary>
    /// <remarks>
    ///     Implement this interface to enable Account link feature.
    ///     Use <c>IServiceCollection.AddOwnId(builder => { builder.UseUserHandlerWithCustomProfile<![CDATA[<MyHandler>]]>(); })</c>
    ///     for this purpose
    /// </remarks>
    /// <typeparam name="TProfile">User Profile</typeparam>
    public interface IAccountLinkHandler<TProfile> where TProfile : class
    {
        Task<string> GetCurrentUserIdAsync(HttpRequest request);

        Task<TProfile> GetUserProfileAsync(string did);

        Task OnLink(IUserProfileFormContext<TProfile> context);
    }
}