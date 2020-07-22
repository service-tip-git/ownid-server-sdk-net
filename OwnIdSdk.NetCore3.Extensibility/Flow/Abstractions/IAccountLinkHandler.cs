using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions
{
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
        Task<string> GetCurrentUserIdAsync(string payload);

        Task<TProfile> GetUserProfileAsync(string did);

        Task OnLink(IUserProfileFormContext<TProfile> context);
    }
}