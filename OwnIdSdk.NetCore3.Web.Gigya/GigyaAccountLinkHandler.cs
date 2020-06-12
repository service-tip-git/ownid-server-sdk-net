using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaAccountLinkHandler : IAccountLinkHandler<GigyaUserProfile>
    {
        private const string ApiKeyPayloadKey = "apiKey";
        private readonly GigyaConfiguration _configuration;
        private readonly GigyaRestApiClient _restApiClient;

        public GigyaAccountLinkHandler(GigyaRestApiClient restApiClient, GigyaConfiguration configuration)
        {
            _restApiClient = restApiClient;
            _configuration = configuration;
        }

        public async Task<string> GetCurrentUserIdAsync(HttpRequest request)
        {
            var jwt = (await JsonSerializer.DeserializeAsync<LinkChallengeData>(request.Body))?.Data?.Jwt;

            if (string.IsNullOrEmpty(jwt))
                throw new Exception("No JWT was found");
            
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(jwt)) throw new Exception("invalid jwt");

            var rsaSecurityKey = await _restApiClient.GetPublicKey();

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

            var token = tokenHandler.ReadJwtToken(jwt);

            if (!token.Payload.ContainsKey(ApiKeyPayloadKey) ||
                (string) token.Payload[ApiKeyPayloadKey] != _configuration.ApiKey)
                throw new Exception("jwt was created with different apiKey");

            return token.Subject;
        }

        public async Task<GigyaUserProfile> GetUserProfileAsync(string did)
        {
            var accountInfo = await _restApiClient.GetUserInfoByUid(did);

            if (accountInfo.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.ErrorCode}: {accountInfo.ErrorMessage}");

            // TODO: refactor. add type parameter
            // TODO: use reflection to fill fields
            return new GigyaUserProfile
            {
                Email = TryGetValue(accountInfo.Profile, "email"),
                FirstName = TryGetValue(accountInfo.Profile, "firstName"),
                LastName = TryGetValue(accountInfo.Profile, "lastName"),
                Nickname = TryGetValue(accountInfo.Profile, "nickName")
            };
        }

        public async Task OnLink(IUserProfileFormContext<GigyaUserProfile> context)
        {
            var accountInfo = await _restApiClient.GetUserInfoByUid(context.DID);

            if (accountInfo.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.ErrorCode}: {accountInfo.ErrorMessage}");

            var setAccountMessage =
                await _restApiClient.SetAccountInfo(context.DID, context.Profile, new {pubKey = context.PublicKey});

            if (setAccountMessage.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.ErrorCode}: {accountInfo.ErrorMessage}");
        }

        private string TryGetValue(Dictionary<string, string> data, string key)
        {
            return data.TryGetValue(key, out var value) ? value : null;
        }
    }
}