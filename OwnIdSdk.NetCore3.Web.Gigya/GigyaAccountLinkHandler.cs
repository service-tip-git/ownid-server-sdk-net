using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Link;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaAccountLinkHandler<TProfile> : IAccountLinkHandler
        where TProfile : class, IGigyaUserProfile
    {
        private const string ApiKeyPayloadKey = "apiKey";
        private readonly GigyaConfiguration _configuration;
        private readonly IOwnIdCoreConfiguration _ownIdConfiguration;
        private readonly GigyaRestApiClient<TProfile> _restApiClient;

        public GigyaAccountLinkHandler(GigyaRestApiClient<TProfile> restApiClient, GigyaConfiguration configuration,
            IOwnIdCoreConfiguration ownIdConfiguration)
        {
            _restApiClient = restApiClient;
            _configuration = configuration;
            _ownIdConfiguration = ownIdConfiguration;
        }

        public async Task<LinkState> GetCurrentUserLinkStateAsync(string payload)
        {
            var jwt = OwnIdSerializer.Deserialize<JwtContainer>(payload)?.Jwt;

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

            var did = token.Subject;
            
            var accountInfo = await _restApiClient.GetUserInfoByUid(did);

            if (accountInfo.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.GetFailureMessage()}");

            return new LinkState(did, (uint) accountInfo.Data.Connections.Count);
        }
        
        public async Task OnLink(string did, string publicKey)
        {
            var accountInfo = await _restApiClient.GetUserInfoByUid(did);

            if (accountInfo.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.GetFailureMessage()}");

            if (accountInfo.Data.Connections.Count >= _ownIdConfiguration.MaximumNumberOfConnectedDevices)
                throw new Exception(
                    $"Gigya.OnLink error -> maximum number ({_ownIdConfiguration.MaximumNumberOfConnectedDevices}) of linked devices reached");

            // add new public key to 
            accountInfo.Data.Connections.Add(new OwnIdConnection {PublicKey = publicKey});

            var setAccountMessage =
                await _restApiClient.SetAccountInfo<TProfile>(did, data: accountInfo.Data);

            if (setAccountMessage.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.setAccountInfo error -> {setAccountMessage.GetFailureMessage()}");
        }
    }
}