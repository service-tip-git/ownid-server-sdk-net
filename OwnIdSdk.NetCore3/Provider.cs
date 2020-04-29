using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3
{
    public class Provider
    {
        private readonly ICacheStore _cacheStore;
        private readonly ProviderConfiguration _configuration;

        public Provider(ICacheStore cacheStore, ProviderConfiguration configuration)
        {
            _cacheStore = cacheStore;
            _configuration = configuration;
        }

        public string GenerateContext()
        {
            // TODO: change to proper context generation
            return Guid.NewGuid().ToString();
        }

        public string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetDeepLink(string context, ChallengeType challengeType)
        {
            var applicationUrl = new UriBuilder(_configuration.OwnIdApplicationUrl);
            var query = HttpUtility.ParseQueryString(applicationUrl.Query);
            query["q"] = HttpUtility.UrlEncode(GenerateCallbackUrl(context).ToString());
            query["type"] = challengeType.ToString().ToLowerInvariant();
            applicationUrl.Query = query.ToString();
            return applicationUrl.ToString();
        }

        private Uri GenerateCallbackUrl(string context)
        {
            return new Uri(new Uri(_configuration.CallbackUrl), $"ownid/{context}/challenge");
        }

        public string GenerateChallengeJwt(string context)
        {
            var rsaSecurityKey = new RsaSecurityKey(_configuration.JwtSignCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)),
                new JwtPayload(null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), DateTime.UtcNow)
                {
                    {"jti", context},
                    {"callback", GenerateCallbackUrl(context)},
                    {
                        "requester", new
                        {
                            did = _configuration.Requester.DID,
                            pubKey = Convert.ToBase64String(_configuration.JwtSignCredentials.ExportRSAPublicKey()),
                            name = _configuration.Requester.Name,
                            icon = _configuration.Requester.Icon,
                            description = _configuration.Requester.Description
                        }
                    },
                    {
                        "requestedFields", _configuration.ProfileFields.Select(x => new
                        {
                            type = x.Type.ToString().ToLowerInvariant(),
                            key = x.Key,
                            label = x.Label,
                            placeholder = x.Placeholder,
                            required = x.IsRequired
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
            var rsaSecurityKey = new RsaSecurityKey(RsaHelper.LoadKeys(user.PublicKey));

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
            
        }

        public bool IsContextValid(string context)
        {
            return Regex.IsMatch(context,
                "^([0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})$");
        }
    }
}