using System.Threading.Tasks;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Link;

namespace OwnID.Extensibility.Flow.Abstractions
{
    /// <summary>
    ///     Describes base Account link handling operations
    /// </summary>
    /// <remarks>
    ///     Implement this interface to enable Account link feature.
    /// </remarks>
    public interface IAccountLinkHandler
    {
        /// <summary>
        ///     Gets user links information
        /// </summary>
        /// <param name="payload">Account link payload</param>
        /// <returns>User links information</returns>
        Task<LinkState> GetCurrentUserLinkStateAsync(string payload);

        /// <summary>
        ///     Links OwnIdConnection to user account
        /// </summary>
        /// <param name="did">User unique identifier</param>
        /// <param name="connection">Connection credentials</param>
        Task OnLinkAsync(string did, OwnIdConnection connection);
    }
}