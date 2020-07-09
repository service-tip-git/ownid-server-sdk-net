using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensions;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3
{
    /// <summary>
    ///     Base OwnId logic provider
    /// </summary>
    public class OwnIdProvider
    {
        private readonly ICacheStore _cacheStore;
        private readonly ILocalizationService _localizationService;
        private readonly IOwnIdCoreConfiguration _ownIdCoreConfiguration;

        /// <summary>
        /// </summary>
        /// <param name="ownIdCoreConfiguration">Core configuration to be used</param>
        /// <param name="cacheStore"><see cref="CacheItem" /> store</param>
        /// <param name="localizationService">Optional(only if localization is needed). Localization service</param>
        public OwnIdProvider([NotNull] IOwnIdCoreConfiguration ownIdCoreConfiguration, [NotNull] ICacheStore cacheStore,
            ILocalizationService localizationService)
        {
            _cacheStore = cacheStore;
            _localizationService = localizationService;
            _ownIdCoreConfiguration = ownIdCoreConfiguration;
        }

        /// <summary>
        ///     Generates new unique identifier
        /// </summary>
        public string GenerateContext()
        {
            return Guid.NewGuid().ToShortString();
        }

        /// <summary>
        ///     Generates nonce
        /// </summary>
        public string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        public string GenerateUserDid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Creates redirection link to OwnId application
        /// </summary>
        /// <param name="context">Challenge Unique identifierChallenge Unique identifier</param>
        /// <param name="challengeType">
        ///     <see cref="ChallengeType" />
        /// </param>
        /// <returns>Well formatted url string</returns>
        public string GetDeepLink(string context, ChallengeType challengeType)
        {
            var deepLink = new UriBuilder(_ownIdCoreConfiguration.OwnIdApplicationUrl);
            var query = HttpUtility.ParseQueryString(deepLink.Query);
            query["t"] = challengeType.ToString("D");
            var callbackUrl = GenerateCallbackUrl(context, challengeType);
            query["q"] = $"{callbackUrl.Authority}{callbackUrl.PathAndQuery}";

            deepLink.Query = query.ToString() ?? string.Empty;
            return deepLink.Uri.ToString();
        }

        private Uri GenerateCallbackUrl(string context, ChallengeType challengeType)
        {
            var path = "";

            if (!string.IsNullOrEmpty(_ownIdCoreConfiguration.CallbackUrl.PathAndQuery))
                path = _ownIdCoreConfiguration.CallbackUrl.PathAndQuery.EndsWith("/")
                    ? _ownIdCoreConfiguration.CallbackUrl.PathAndQuery
                    : _ownIdCoreConfiguration.CallbackUrl.PathAndQuery + "/";

            String action;
            switch (challengeType)
            {
                case ChallengeType.Link:
                    action = "link";
                    break;
                case ChallengeType.Recover:
                    action = "recover";
                    break;
                default:
                    action = "challenge";
                    break;
            }

            return new Uri(_ownIdCoreConfiguration.CallbackUrl, $"{path}ownid/{context}/{action}");
        }

        /// <summary>
        /// Generates JWT with configuration and user profile for account linking process 
        /// </summary>
        /// <param name="context">Challenge Unique identifier</param>
        /// <param name="challengeType">Requested <see cref="ChallengeType" /></param>
        /// <param name="did">User unique identity</param>
        /// <param name="profile">User profile</param>
        /// <param name="locale">Optional. Content locale</param>
        /// <returns>Base64 encoded string that contains JWT with hash</returns>
        public (string Jwt, string Hash) GenerateProfileDataJwt(string context, ChallengeType challengeType, string did, object profile,
            string locale = null)
        {
            var data = GetBaseConfigFieldsDictionary(context, challengeType, did, locale)
                .Union(GetProfileDataDictionary(profile))
                .ToDictionary(x => x.Key, x => x.Value);

            return GenerateDataJwt(data);
        }


        /// <summary>
        ///     Creates JWT challenge with requested information by OwnId app
        /// </summary>
        /// <param name="context">Challenge Unique identifier</param>
        /// <param name="challengeType">Requested <see cref="ChallengeType" /></param>
        /// <param name="locale">Optional. Content locale</param>
        /// <returns>Base64 encoded string that contains JWT with hash</returns>
        public (string Jwt, string Hash) GenerateChallengeJwt(string context, ChallengeType challengeType, string locale = null)
        {
            var data = GetBaseConfigFieldsDictionary(context, challengeType, GenerateUserDid(), locale);

            return GenerateDataJwt(data);
        }

        /// <summary>
        ///     Decodes provided by OwnId application JWT with data
        /// </summary>
        /// <param name="jwt">Base64 JWT string </param>
        /// <typeparam name="TData">Data type used as model for deserialization</typeparam>
        /// <returns>Context(challenge unique identifier) with <see cref="UserProfileData" /></returns>
        /// <exception cref="Exception">If something went wrong during token validation</exception>
        public (string, TData) GetDataFromJwt<TData>(string jwt) where TData : ISignedData
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(jwt)) throw new Exception("invalid jwt");

            var token = tokenHandler.ReadJwtToken(jwt);
            var data = JsonSerializer.Deserialize<TData>(token.Payload["data"].ToString());
            // TODO: add type of challenge
            using var sr = new StringReader(data.PublicKey);
            var rsaSecurityKey = new RsaSecurityKey(RsaHelper.LoadKeys(sr));

            try
            {
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    IssuerSigningKey = rsaSecurityKey,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false
                    // TODO: add issuer to token for validation
                }, out _);
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new Exception($"Token failed validation: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Token was invalid: {ex.Message}");
            }

            return (token.Id, data);
        }

        /// <summary>
        ///     Creates auth flow session item and saves it by <paramref name="context" /> into <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <param name="challengeType">Requested challenge type</param>
        /// <param name="did">User unique identity, should be null for register or login</param>
        /// <param name="payload">payload</param>
        public async Task CreateAuthFlowSessionItemAsync(string context, string nonce, ChallengeType challengeType,
            string did = null, string payload = null)
        {
            await _cacheStore.SetAsync(context, new CacheItem
            {
                ChallengeType = challengeType,
                Nonce = nonce,
                Context = context,
                DID = did,
                Payload = payload,
            }
            , TimeSpan.FromMilliseconds(_ownIdCoreConfiguration.CacheExpirationTimeout));
        }

        /// <summary>
        /// Sets Web App request token to check in with the next request
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="token">Web App request token</param>
        /// <exception cref="ArgumentException">If no <see cref="CacheItem" /> was found with <paramref name="context" /></exception>
        public async Task SetRequestTokenAsync(string context, string token)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.RequestToken = token;
            cacheItem.Status = CacheItemStatus.Processing;
            await _cacheStore.SetAsync(context, cacheItem, TimeSpan.FromMilliseconds(_ownIdCoreConfiguration.CacheExpirationTimeout));
        }

        /// <summary>
        /// Sets Web App response token to check in with the next request
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="token">Web App response token</param>
        /// <exception cref="ArgumentException">If no <see cref="CacheItem" /> was found with <paramref name="context" /></exception>
        public async Task SetResponseTokenAsync(string context, string token)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.ResponseToken = token;
            cacheItem.Status = CacheItemStatus.Processing;
            await _cacheStore.SetAsync(context, cacheItem, TimeSpan.FromMilliseconds(_ownIdCoreConfiguration.CacheExpirationTimeout));
        }

        /// <summary>
        ///     Try to find auth flow session item by <paramref name="context" /> in <see cref="ICacheStore" /> mark it as finish
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="did">User unique identifier</param>
        /// <param name="challengeType"></param>
        /// <exception cref="ArgumentException">
        ///     If no <see cref="CacheItem" /> was found with <paramref name="context" />
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If you try to finish session with different user DID
        /// </exception>
        public async Task FinishAuthFlowSessionAsync(string context, string did)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            if (cacheItem.ChallengeType == ChallengeType.Link && cacheItem.DID != did)
                throw new ArgumentException($"Wrong user for linking {did}");

            cacheItem.DID = did;
            cacheItem.Status = CacheItemStatus.Finished;
            await _cacheStore.SetAsync(context, cacheItem, TimeSpan.FromMilliseconds(_ownIdCoreConfiguration.CacheExpirationTimeout));
        }

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="nonce" /> and <paramref name="context" /> in
        ///     <see cref="ICacheStore" /> and remove item if find operation was successful
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <returns>
        ///     <see cref="CacheItemStatus"/> and <c>did</c> if <see cref="CacheItem" /> was found, otherwise null
        /// </returns>
        public async Task<(CacheItemStatus Status, string DID)?> PopFinishedAuthFlowSessionAsync(string context, string nonce)
        {
            var cacheItem = await _cacheStore.GetAsync(context);
            
            if (cacheItem == null
                || cacheItem.Nonce != nonce
                || cacheItem.Status == CacheItemStatus.Finished && String.IsNullOrEmpty(cacheItem.DID)
            )
            {
                return null;
            }

            // If not finished - return just status
            if (cacheItem.Status != CacheItemStatus.Finished) 
                return (cacheItem.Status, null);
            
            // If finished - clear cache
            await _cacheStore.RemoveAsync(context);

            return (Status: CacheItemStatus.Finished, cacheItem.DID);
        }

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="context" /> in <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <returns><see cref="CacheItem" /> or null</returns>
        public async Task<CacheItem> GetCacheItemByContextAsync(string context)
        {
            return (await _cacheStore.GetAsync(context))?.Clone() as CacheItem;
        }

        /// <summary>
        ///     Verifies if provided <paramref name="context" /> is valid
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <returns>True if valid</returns>
        public bool IsContextFormatValid(string context)
        {
            return Regex.IsMatch(context, "^([a-zA-Z0-9_-]{22})$");
        }

        private (string Jwt, string Hash) GenerateDataJwt(Dictionary<string, object> data, TimeSpan? expiration = null)
        {
            var rsaSecurityKey = new RsaSecurityKey(_ownIdCoreConfiguration.JwtSignCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();

            //TODO: should be received from the user's phone
            var issuedAt = DateTime.UtcNow.Add(TimeSpan.FromHours(-1));
            var notBefore = issuedAt;
            var expires = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(1));

            var payload = new JwtPayload(null, null, null, notBefore, expires, issuedAt);

            foreach (var (key, value) in data)
            {
                payload.Add(key, value);
            }

            var jwt = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)), payload);

            var tokenStr = tokenHandler.WriteToken(jwt);

            using var sha1 = new SHA1Managed();

            var b64 = Encoding.UTF8.GetBytes(tokenStr);
            var hash = sha1.ComputeHash(b64);

            return (tokenStr, Convert.ToBase64String(hash));
        }

        private Dictionary<string, object> GetProfileDataDictionary(object profile)
        {
            return new Dictionary<string, object> { { "profile", profile } };
        }

        private Dictionary<string, object> GetBaseConfigFieldsDictionary(string context, ChallengeType challengeType, string did, string locale = null)
        {
            var data = new Dictionary<string, object>
            {
                {"jti", context},
                {"locale", locale},
                {"did", did},
                {"callback", GenerateCallbackUrl(context, challengeType)},
                {
                    "requester", new
                    {
                        did = _ownIdCoreConfiguration.DID,
                        pubKey = RsaHelper.ExportPublicKeyToPkcsFormattedString(_ownIdCoreConfiguration
                            .JwtSignCredentials),
                        name = Localize(_ownIdCoreConfiguration.Name),
                        icon = _ownIdCoreConfiguration.Icon,
                        description = Localize(_ownIdCoreConfiguration.Description)
                    }
                },
                {
                    // TODO : PROFILE
                    "requestedFields", _ownIdCoreConfiguration.ProfileConfiguration.ProfileFieldMetadata.Select(x =>
                    {
                        var label = Localize(x.Label, true);

                        return new
                        {
                            type = x.Type,
                            key = x.Key,
                            label,
                            placeholder = Localize(x.Placeholder, true),
                            validators = x.Validators.Select(v => new
                            {
                                type = v.Type,
                                errorMessage = string.Format(v.NeedsInternalLocalization
                                    ? Localize(v.GetErrorMessageKey(), true)
                                    : v.GetErrorMessageKey(), label)
                            })
                        };
                    })
                }
            };

            return data;
        }

        private string Localize(string key, bool defaultAsAlternative = false)
        {
            return _localizationService?.GetLocalizedString(key) ?? key;
        }
    }
}