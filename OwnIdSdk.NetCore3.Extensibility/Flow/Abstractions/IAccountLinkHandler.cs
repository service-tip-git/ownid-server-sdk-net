using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Link;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions
{
    /// <summary>
    ///     Describes base Account link handling operations
    /// </summary>
    /// <remarks>
    ///     Implement this interface to enable Account link feature.
    /// </remarks>
    public interface IAccountLinkHandler
    {
        Task<LinkState> GetCurrentUserLinkStateAsync(string payload);

        Task OnLinkAsync(string did, string publicKey, string fido2CredentialId = null,
            uint? fido2SignatureCounter = null);
    }
}