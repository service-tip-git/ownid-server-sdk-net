using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;

namespace OwnIdSdk.NetCore3.Services
{
    public class CacheItemService : ICacheItemService
    {
        private readonly ICacheStore _cacheStore;
        private readonly TimeSpan _expirationTimeout;

        public CacheItemService(ICacheStore cacheStore, IOwnIdCoreConfiguration coreConfiguration)
        {
            _cacheStore = cacheStore;
            _expirationTimeout = TimeSpan.FromMilliseconds(coreConfiguration.CacheExpirationTimeout);
        }

        /// <summary>
        ///     Creates auth flow session item and saves it by <paramref name="context" /> into <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="challengeType">Requested challenge type</param>
        /// <param name="flowType">Flow type for OwnID process</param>
        /// <param name="did">User unique identity, should be null for register or login</param>
        /// <param name="payload">payload</param>
        public async Task CreateAuthFlowSessionItemAsync(string context, string nonce, ChallengeType challengeType,
            FlowType flowType,
            string did = null, string payload = null)
        {
            await _cacheStore.SetAsync(context, new CacheItem
            {
                ChallengeType = challengeType,
                Nonce = nonce,
                Context = context,
                DID = did,
                Payload = payload,
                FlowType = flowType
            }, _expirationTimeout);
        }

        /// <summary>
        ///     Sets Web App request/response token to check with the next request
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="requestToken">Web App request token</param>
        /// <param name="responseToken">Server-side response token</param>
        /// <exception cref="ArgumentException">
        ///     If no <see cref="CacheItem" /> was found with <paramref name="context" />
        /// </exception>
        public async Task SetSecurityTokensAsync(string context, string requestToken, string responseToken)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.RequestToken = requestToken;
            cacheItem.ResponseToken = responseToken;

            // TODO: move somewhere and rework logic
            if (cacheItem.Status == CacheItemStatus.Initiated)
                cacheItem.Status = CacheItemStatus.Started;

            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        /// <summary>
        ///     Sets security code to cache item and changes status to <see cref="CacheItemStatus.WaitingForApproval" />
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> SetSecurityCodeAsync(string context)
        {
            var random = new Random();
            var pin = random.Next(0, 9999).ToString("D4");
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            if (cacheItem.Status != CacheItemStatus.Initiated && cacheItem.Status != CacheItemStatus.Started)
                throw new ArgumentException(
                    $"Wrong status '{cacheItem.Status.ToString()}' for cache item with context '{context}' to set PIN");

            if (cacheItem.ConcurrentId != null)
                throw new ArgumentException($"Context '{context}' is already modified with PIN");

            cacheItem.ConcurrentId = Guid.NewGuid().ToString();
            cacheItem.SecurityCode = pin;
            cacheItem.Status = CacheItemStatus.WaitingForApproval;

            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);

            return pin;
        }

        /// <summary>
        ///     Sets approval result
        /// </summary>
        /// <param name="context">Challenge Unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="isApproved">True if approved</param>
        /// <exception cref="ArgumentException">Cache item was not found</exception>
        /// <exception cref="ArgumentException">Cache item has incorrect status to set resolution</exception>
        public async Task SetApprovalResolutionAsync(string context, string nonce, bool isApproved)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null || cacheItem.Context != context || cacheItem.Nonce != nonce)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            if (cacheItem.Status != CacheItemStatus.WaitingForApproval)
                throw new ArgumentException($"Incorrect status={cacheItem.Status.ToString()} for approval '{context}'");

            cacheItem.Status = isApproved ? CacheItemStatus.Approved : CacheItemStatus.Declined;
            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        public async Task SetFido2DataAsync(string context, string publicKey, uint fido2Counter,
            string fido2CredentialId)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null || cacheItem.Context != context)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            if (cacheItem.FlowType != FlowType.Fido2PartialLogin
                && cacheItem.FlowType != FlowType.Fido2PartialRegister
                && cacheItem.FlowType != FlowType.Fido2LinkWithPin
                && cacheItem.FlowType != FlowType.Fido2RecoverWithPin
            )
                throw new ArgumentException(
                    $"Can not set Fido2 information for the flow not related to Fido2. Current flow: {cacheItem.FlowType} Context: '{context}'");

            if (cacheItem.Status != CacheItemStatus.Initiated && cacheItem.Status != CacheItemStatus.Started)
                throw new ArgumentException(
                    $"Incorrect status={cacheItem.Status.ToString()} for setting public key for context '{context}'");

            cacheItem.PublicKey = publicKey;
            cacheItem.Fido2SignatureCounter = fido2Counter;
            cacheItem.Fido2CredentialId = fido2CredentialId;

            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        /// <summary>
        ///     Try to find auth flow session item by <paramref name="context" /> in <see cref="ICacheStore" /> mark it as finish
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="did">User unique identifier</param>
        /// <param name="publicKey">User public key</param>
        /// <exception cref="ArgumentException">
        ///     If no <see cref="CacheItem" /> was found with <paramref name="context" />
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If you try to finish session with different user DID
        /// </exception>
        public async Task FinishAuthFlowSessionAsync(string context, string did, string publicKey)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            if (cacheItem.ChallengeType == ChallengeType.Link && cacheItem.DID != did)
                throw new ArgumentException($"Wrong user for linking {did}");

            if (cacheItem.HasFinalState)
                throw new ArgumentException(
                    $"Cache item with context='{context}' has final status={cacheItem.Status.ToString()}");

            cacheItem.DID = did;
            cacheItem.PublicKey = publicKey;
            cacheItem.Status = CacheItemStatus.Finished;
            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        /// <summary>
        ///     Sets recovery data for auth-only flow
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="recoveryData">Data for recovery</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task SetRecoveryDataAsync(string context, string recoveryData)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.RecoveryData = recoveryData;

            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        public async Task SetPasswordlessStateAsync(string context, string encryptionToken,
            string recoveryToken = null)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.PasswordlessRecoveryToken = recoveryToken;
            cacheItem.PasswordlessEncToken = encryptionToken;
            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }
        
        public async Task SetWebAppStateAsync(string context, string encryptionToken,
            string recoveryToken = null)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.WebAppRecoveryToken = recoveryToken;
            cacheItem.WebAppEncToken = encryptionToken;
            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="nonce" /> and <paramref name="context" /> in
        ///     <see cref="ICacheStore" /> and remove item if find operation was successful
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <returns>
        ///     <see cref="CacheItemStatus" /> and <c>did</c> if <see cref="CacheItem" /> was found, otherwise null
        /// </returns>
        public async Task<CacheItem> PopFinishedAuthFlowSessionAsync(
            string context,
            string nonce)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null || cacheItem.Nonce != nonce)
                return null;

            // If finished - clear cache
            if (cacheItem.Status == CacheItemStatus.Finished)
                await _cacheStore.RemoveAsync(context);

            return cacheItem.Clone() as CacheItem;
        }

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="context" /> in <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <returns><see cref="CacheItem" /> or null</returns>
        public async Task<CacheItem> GetCacheItemByContextAsync(string context)
        {
            var item = (await _cacheStore.GetAsync(context))?.Clone() as CacheItem;

            if (item == null)
                throw new InternalLogicException($"No cache item was found with context={context}");

            return item;
        }

        public async Task UpdateFlowAsync(string context, FlowType flowType, ChallengeType challengeType)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.FlowType = flowType;
            cacheItem.ChallengeType = challengeType;

            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);
        }

        public async Task<CacheItem> FinishFlowWithErrorAsync(string context, string errorMessage)
        {
            var cacheItem = await _cacheStore.GetAsync(context);
            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.Status = CacheItemStatus.Finished;
            cacheItem.Error = errorMessage;

            await _cacheStore.SetAsync(context, cacheItem, _expirationTimeout);

            return cacheItem;
        }
    }
}