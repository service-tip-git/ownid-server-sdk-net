using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
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
            query["t"] = challengeType.ToString().Substring(0, 1).ToLowerInvariant();
            var callbackUrl = GenerateCallbackUrl(context);
            query["q"] = $"{callbackUrl.Authority}{callbackUrl.PathAndQuery}";

            deepLink.Query = query.ToString() ?? string.Empty;
            return deepLink.Uri.ToString();
        }

        private Uri GenerateCallbackUrl(string context)
        {
            var path = "";

            if (!string.IsNullOrEmpty(_ownIdCoreConfiguration.CallbackUrl.PathAndQuery))
                path = _ownIdCoreConfiguration.CallbackUrl.PathAndQuery.EndsWith("/")
                    ? _ownIdCoreConfiguration.CallbackUrl.PathAndQuery
                    : _ownIdCoreConfiguration.CallbackUrl.PathAndQuery + "/";

            return new Uri(_ownIdCoreConfiguration.CallbackUrl, path + $"ownid/{context}/challenge");
        }

        /// <summary>
        ///     Creates JWT challenge with requested information by OwnId app
        /// </summary>
        /// <param name="context">Challenge Unique identifier</param>
        /// <param name="locale">Optional. Content locale</param>
        /// <returns>Base64 encoded string that contains JWT</returns>
        public string GenerateChallengeJwt(string context, string locale = null)
        {
            var rsaSecurityKey = new RsaSecurityKey(_ownIdCoreConfiguration.JwtSignCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)),
                new JwtPayload(null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), DateTime.UtcNow)
                {
                    {"jti", context},
                    {"locale", locale},
                    {"callback", GenerateCallbackUrl(context)},
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
                });

            return tokenHandler.WriteToken(jwt);
        }

        /// <summary>
        ///     Decodes provided by OwnId application JWT with user data
        /// </summary>
        /// <param name="jwt">Base64 JWT string </param>
        /// <returns>Context(challenge unique identifier) with <see cref="UserProfileData" /></returns>
        /// <exception cref="Exception">If something went wrong during token validation</exception>
        public (string, UserProfileData) GetProfileDataFromJwt(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(jwt)) throw new Exception("invalid jwt");

            var token = tokenHandler.ReadJwtToken(jwt);
            var user = JsonSerializer.Deserialize<UserProfileData>(token.Payload["user"].ToString());
            // TODO: add type of challenge
            using var sr = new StringReader(user.PublicKey);
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

            return (token.Id, user);
        }

        /// <summary>
        ///     Stores <paramref name="nonce" /> by <paramref name="context" /> into <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        public async Task StoreNonceAsync(string context, string nonce)
        {
            await _cacheStore.SetAsync(context, new CacheItem
            {
                Nonce = nonce
            });
        }

        /// <summary>
        ///     Stores <paramref name="did" /> by <paramref name="context" /> into <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="did">User unique identifier</param>
        /// <exception cref="ArgumentException">
        ///     If no <see cref="CacheItem" /> was found with <paramref name="context" />
        /// </exception>
        public async Task SetDIDAsync(string context, string did)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.DID = did;
            await _cacheStore.SetAsync(context, cacheItem);
        }

        /// <summary>
        ///     Tries to find <see cref="CacheItem" /> by <paramref name="nonce" /> and <paramref name="context" /> in
        ///     <see cref="ICacheStore" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        /// <param name="nonce">Nonce</param>
        /// <returns>
        ///     <c>isSuccess = true</c> and <c>did</c> if <see cref="CacheItem" /> was found or <c>isSuccess = false</c> there
        ///     is no such <see cref="CacheItem" />
        /// </returns>
        public async Task<(bool isSuccess, string did)> GetDIDAsync(string context, string nonce)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            return cacheItem?.Nonce != nonce || string.IsNullOrEmpty(cacheItem?.DID)
                ? (false, null)
                : (true, cacheItem?.DID);
        }

        /// <summary>
        ///     Removes <see cref="CacheItem" /> from <see cref="ICacheStore" /> by <paramref name="context" />
        /// </summary>
        /// <param name="context">Challenge unique identifier</param>
        public async Task RemoveContextAsync(string context)
        {
            await _cacheStore.RemoveAsync(context);
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

        private string Localize(string key, bool defaultAsAlternative = false)
        {
            return _localizationService?.GetLocalizedString(key) ?? key;
        }
    }
}