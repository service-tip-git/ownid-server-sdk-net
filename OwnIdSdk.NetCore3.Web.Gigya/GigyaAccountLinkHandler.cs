using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaAccountLinkHandler<TProfile> : IAccountLinkHandler<TProfile>
        where TProfile : class, IGigyaUserProfile
    {
        private const string ApiKeyPayloadKey = "apiKey";
        private readonly GigyaConfiguration _configuration;
        private readonly IOwnIdCoreConfiguration _ownIdConfiguration;
        private readonly GigyaRestApiClient<TProfile> _restApiClient;

        public GigyaAccountLinkHandler(
            GigyaRestApiClient<TProfile> restApiClient
            , GigyaConfiguration configuration
            , IOwnIdCoreConfiguration ownIdConfiguration
        )
        {
            _restApiClient = restApiClient;
            _configuration = configuration;
            _ownIdConfiguration = ownIdConfiguration;
        }

        public async Task<string> GetCurrentUserIdAsync(string payload)
        {
            var jwt = JsonSerializer.Deserialize<JwtContainer>(payload)?.Jwt;

            if (string.IsNullOrEmpty(jwt))
                throw new Exception("No JWT was found in HttpRequest");

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(jwt)) throw new Exception("Invalid jwt");

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
                throw new Exception("Jwt was created for different apiKey");

            return token.Subject;
        }

        public async Task<TProfile> GetUserProfileAsync(string did)
        {
            var accountInfo = await _restApiClient.GetUserInfoByUid(did);

            if (accountInfo.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.GetFailureMessage()}");

            return accountInfo.Profile;
        }

        public async Task OnLink(IUserProfileFormContext<TProfile> context)
        {
            var accountInfo = await _restApiClient.GetUserInfoByUid(context.DID);

            if (accountInfo.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.GetFailureMessage()}");

            if (accountInfo.Data.Connections.Count >= _ownIdConfiguration.MaximumNumberOfConnectedDevices)
                throw new Exception(
                    $"Gigya.OnLink error -> maximum number ({_ownIdConfiguration.MaximumNumberOfConnectedDevices}) of linked devices reached");

            // add new public key to 
            accountInfo.Data.Connections.Add(new OwnIdConnection {PublicKey = context.PublicKey});

            var setAccountMessage =
                await _restApiClient.SetAccountInfo(context.DID, context.Profile, accountInfo.Data);

            if (setAccountMessage.ErrorCode == 403043)
            {
                context.SetError(x => x.Email, setAccountMessage.ErrorMessage);
                return;
            }

            if (setAccountMessage.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.setAccountInfo error -> {setAccountMessage.GetFailureMessage()}");
        }
    }
}