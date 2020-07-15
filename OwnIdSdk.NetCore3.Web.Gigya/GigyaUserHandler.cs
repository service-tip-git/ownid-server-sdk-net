using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaUserHandler : IUserHandler<GigyaUserProfile>
    {
        private readonly GigyaConfiguration _configuration;
        private readonly ILogger<GigyaUserHandler> _logger;
        private readonly GigyaRestApiClient _restApiClient;

        public GigyaUserHandler(GigyaRestApiClient restApiClient, GigyaConfiguration configuration,
            ILogger<GigyaUserHandler> logger)
        {
            _restApiClient = restApiClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResult<object>> OnSuccessLoginAsync(string did)
        {
            if (_configuration.LoginType == GigyaLoginType.Session)
            {
                var loginResponse = await _restApiClient.NotifyLogin(did, "browser");

                if (loginResponse.SessionInfo == null || loginResponse.ErrorCode != 0)
                    return new LoginResult<object>
                    {
                        HttpCode = (int) HttpStatusCode.Unauthorized,
                        Data = new
                        {
                            status = false,
                            errorMessage = $"Gigya: {loginResponse.GetFailureMessage()}"
                        }
                    };

                return new LoginResult<object>
                {
                    HttpCode = (int) HttpStatusCode.OK,
                    Data = new
                    {
                        status = true,
                        sessionInfo = loginResponse.SessionInfo,
                        identities = loginResponse.Identities.FirstOrDefault()
                    }
                };
            }

            var jwtResponse = await _restApiClient.GetJwt(did);

            if (jwtResponse.IdToken == null || jwtResponse.ErrorCode != 0)
                return new LoginResult<object>
                {
                    HttpCode = (int) HttpStatusCode.Unauthorized,
                    Data = new
                    {
                        status = false,
                        errorMessage = $"Gigya: {jwtResponse.GetFailureMessage()}"
                    }
                };

            return new LoginResult<object>
            {
                HttpCode = (int) HttpStatusCode.Unauthorized,
                Data = new
                {
                    status = true,
                    Data = new
                    {
                        status = true,
                        idToken = jwtResponse.IdToken
                    }
                }
            };
        }

        public async Task UpdateProfileAsync(IUserProfileFormContext<GigyaUserProfile> context)
        {
            var getAccountMessage = await _restApiClient.GetUserInfoByUid(context.DID);

            if (getAccountMessage.ErrorCode == 0)
            {
                if (getAccountMessage.Data == null || !getAccountMessage.Data.PubKeys.Any())
                    throw new Exception($"Found gigya user uid={context.DID} without pubKey");
                
                if (!getAccountMessage.Data.PubKeys.Contains(context.PublicKey))
                    throw new Exception("Public key doesn't match any of gigya user keys");

                var setAccountResponse = await _restApiClient.SetAccountInfo(context.DID, context.Profile);

                if (setAccountResponse.ErrorCode == 403043)
                {
                    context.SetError(x => x.Email, setAccountResponse.ErrorMessage);
                    return;
                }

                if (setAccountResponse.ErrorCode > 0)
                {
                    var exProfileSerializedFields = JsonSerializer.Serialize(context.Profile, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        IgnoreNullValues = true
                    });

                    _logger.LogError(
                        $"did: {context.DID}{Environment.NewLine}" +
                        $"profile: {exProfileSerializedFields}{Environment.NewLine}" +
                        $"Gigya.setAccountInfo for EXISTING user error -> {setAccountResponse.GetFailureMessage()}");

                    context.SetGeneralError($"{setAccountResponse.ErrorCode}: {setAccountResponse.ErrorMessage}");
                }

                return;
            }

            // new user
            if (getAccountMessage.ErrorCode != 403005)
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {getAccountMessage.GetFailureMessage()}");

            var loginResponse = await _restApiClient.NotifyLogin(context.DID);

            if (loginResponse.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.notifyLogin error -> {loginResponse.GetFailureMessage()}");

            var setAccountMessage =
                await _restApiClient.SetAccountInfo(context.DID, context.Profile, new AccountData(context.PublicKey));

            if (setAccountMessage.ErrorCode == 403043)
            {
                var removeUserResult = await _restApiClient.DeleteAccountAsync(context.DID);

                if (removeUserResult.ErrorCode != 0)
                    throw new Exception(
                        $"Gigya.deleteAccount with uid={context.DID} error -> {loginResponse.GetFailureMessage()}");

                context.SetError(x => x.Email, setAccountMessage.ErrorMessage);
                return;
            }

            if (setAccountMessage.ErrorCode > 0)
            {
                _logger.LogError($"did: {context.DID}{Environment.NewLine}" +
                                 $"Gigya.setAccountInfo (profile) for NEW user error -> {setAccountMessage.GetFailureMessage()}");

                context.SetGeneralError($"{setAccountMessage.ErrorCode}: {setAccountMessage.ErrorMessage}");
            }
        }
    }
}