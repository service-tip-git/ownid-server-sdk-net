using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Configuration;
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

        public string GetDeepLink(string context)
        {
            // TODO: change to proper link generation
            return $"{_configuration.OwnIdApplicationUrl}/{context}";
        }

        public string GenerateChallengeJwt(string context)
        {
            var rsaSecurityKey = new RsaSecurityKey(_configuration.JwtSignCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenTest = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)),
                new JwtPayload(null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), DateTime.UtcNow)
                {
                    {"jti", context},
                    {"type", "login"},
                    {"callback", _configuration.CallbackUrl},
                    {
                        "requester", new
                        {
                            did = _configuration.Requester.DID,
                            pubKey = _configuration.Requester.PublicKey,
                            name = _configuration.Requester.Name
                        }
                    },
                    {
                        "requestedFields",
                        _configuration.ProfileFields.Select(x => new {label = x.Label, type = x.Type.ToString()})
                            .ToArray()
                    }
                });
            return tokenHandler.WriteToken(tokenTest);
        }

        public Dictionary<ProfileFieldType, string> GetProfileDataFromJwt(string jwt)
        {
            // TODO: parse jwt and get public key
            var rsaSecurityKey = new RsaSecurityKey(RsaHelper.LoadKeys(jwt));

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    IssuerSigningKey = rsaSecurityKey,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true
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

            // TODO; add token parse to profile fields dictionary
            return new Dictionary<ProfileFieldType, string>();
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

            return cacheItem?.Nonce != nonce ? (false, null) : (true, cacheItem?.DID);
        }

        public bool IsContextValid(string context)
        {
            return Regex.IsMatch(context,
                "^([0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})$");
        }
    }
}