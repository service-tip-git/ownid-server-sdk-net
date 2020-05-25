using System;
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
    public class Provider
    {
        private readonly ICacheStore _cacheStore;
        private readonly ILocalizationService _localizationService;
        private readonly OwnIdConfiguration _configuration;

        public Provider(OwnIdConfiguration configuration, ICacheStore cacheStore,
            ILocalizationService localizationService)
        {
            _cacheStore = cacheStore;
            _localizationService = localizationService;
            _configuration = configuration;
        }

        public string GenerateContext()
        {
            return Guid.NewGuid().ToShortString();
        }

        public string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetDeepLink(string context, ChallengeType challengeType)
        {
            var deepLink = new UriBuilder(_configuration.OwnIdApplicationUrl);
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

            if (!string.IsNullOrEmpty(_configuration.CallbackUrl.PathAndQuery))
                path = _configuration.CallbackUrl.PathAndQuery.EndsWith("/")
                    ? _configuration.CallbackUrl.PathAndQuery
                    : _configuration.CallbackUrl.PathAndQuery + "/";

            return new Uri(_configuration.CallbackUrl, path + $"ownid/{context}/challenge");
        }

        public string GenerateChallengeJwt(string context, string locale = null)
        {
            var rsaSecurityKey = new RsaSecurityKey(_configuration.JwtSignCredentials);
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
                            did = _configuration.Requester.DID,
                            pubKey = RsaHelper.ExportPublicKeyToPkcsFormattedString(_configuration.JwtSignCredentials),
                            name = _localizationService.GetLocalizedString(_configuration.Requester.Name),
                            icon = _configuration.Requester.Icon,
                            description = _localizationService.GetLocalizedString(_configuration.Requester.Description)
                        }
                    },
                    {
                        // TODO : PROFILE
                        "requestedFields", _configuration.ProfileConfiguration.ProfileFieldMetadata.Select(x =>
                        {
                            var label = _localizationService.GetLocalizedString(x.Label);

                                return new
                            {
                                type = x.Type,
                                key = x.Key,
                                label = label,
                                placeholder = _localizationService.GetLocalizedString(x.Placeholder),
                                validators = x.Validators.Select(v => new
                                {
                                    type = v.Type,
                                    errorMessage = string.Format(v.NeedsInternalLocalization
                                        ? _localizationService.GetLocalizedString(v.GetErrorMessageKey(), true)
                                        : v.GetErrorMessageKey(), label)
                                })
                            };
                        })
                    }
                });

            return tokenHandler.WriteToken(jwt);
        }

        public (string, UserProfile) GetProfileDataFromJwt(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(jwt)) throw new Exception("invalid jwt");

            var token = tokenHandler.ReadJwtToken(jwt);
            var user = JsonSerializer.Deserialize<UserProfile>(token.Payload["user"].ToString());
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

        public async Task StoreNonceAsync(string context, string nonce)
        {
            await _cacheStore.SetAsync(context, new CacheItem
            {
                Nonce = nonce
            });
        }

        public async Task SetDIDAsync(string context, string did)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            if (cacheItem == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            cacheItem.DID = did;
            await _cacheStore.SetAsync(context, cacheItem);
        }

        public async Task<(bool isSuccess, string did)> GetDIDAsync(string context, string nonce)
        {
            var cacheItem = await _cacheStore.GetAsync(context);

            return cacheItem?.Nonce != nonce || string.IsNullOrEmpty(cacheItem?.DID)
                ? (false, null)
                : (true, cacheItem?.DID);
        }

        public async Task RemoveContextAsync(string context)
        {
            await _cacheStore.RemoveAsync(context);
        }

        public async Task<CacheItem> GetCacheItemByContextAsync(string context)
        {
            return (await _cacheStore.GetAsync(context))?.Clone() as CacheItem;
        }

        public bool IsContextFormatValid(string context)
        {
            return Regex.IsMatch(context, "^([a-zA-Z0-9_-]{22})$");
        }
    }
}