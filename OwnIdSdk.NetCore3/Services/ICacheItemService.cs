using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow;

namespace OwnIdSdk.NetCore3.Services
{
    public interface ICacheItemService
    {
        /// <summary>
        ///     Creates auth flow session item and saves it by <paramref name="context" /> into <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="challengeType">Requested challenge type</param>
        /// <param name="flowType">Flow type for OwnID process</param>
        /// <param name="did">User unique identity, should be null for register or login</param>
        /// <param name="payload">payload</param>
        Task CreateAuthFlowSessionItemAsync(string context, string nonce, ChallengeType challengeType,
            FlowType flowType,
            string did = null, string payload = null);

        /// <summary>
        ///     Sets Web App request/response token to check with the next request
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="requestToken">Web App request token</param>
        /// <param name="responseToken">Server-side response token</param>
        /// <exception cref="ArgumentException">
        ///     If no <see cref="CacheItem" /> was found with <paramref name="context" />
        /// </exception>
        Task SetSecurityTokensAsync(string context, string requestToken, string responseToken);

        /// <summary>
        ///     Sets security code to cache item and changes status to <see cref="CacheItemStatus.WaitingForApproval" />
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        Task<string> SetSecurityCodeAsync(string context);

        /// <summary>
        ///     Sets approval result
        /// </summary>
        /// <param name="context">Challenge Unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="isApproved">True if approved</param>
        /// <exception cref="ArgumentException">Cache item was not found</exception>
        /// <exception cref="ArgumentException">Cache item has incorrect status to set resolution</exception>
        Task SetApprovalResolutionAsync(string context, string nonce, bool isApproved);

        Task SetPublicKeyAsync(string context, string publicKey);

        /// <summary>
        ///     Try to find auth flow session item by <paramref name="context" /> in <see cref="ICacheStore" /> mark it as finish
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="did">User unique identifier</param>
        /// <exception cref="ArgumentException">
        ///     If no <see cref="CacheItem" /> was found with <paramref name="context" />
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If you try to finish session with different user DID
        /// </exception>
        Task FinishAuthFlowSessionAsync(string context, string did);

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="nonce" /> and <paramref name="context" /> in
        ///     <see cref="ICacheStore" /> and remove item if find operation was successful
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <returns>
        ///     <see cref="CacheItemStatus" /> and <c>did</c> if <see cref="CacheItem" /> was found, otherwise null
        /// </returns>
        Task<CacheItem> PopFinishedAuthFlowSessionAsync(
            string context,
            string nonce);

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="context" /> in <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <returns><see cref="CacheItem" /> or null</returns>
        Task<CacheItem> GetCacheItemByContextAsync(string context);
    }
}