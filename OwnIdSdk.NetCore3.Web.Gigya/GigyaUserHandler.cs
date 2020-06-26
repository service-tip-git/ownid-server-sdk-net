using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaUserHandler : IUserHandler<GigyaUserProfile>
    {
        private readonly GigyaRestApiClient _restApiClient;
        private readonly ILogger<GigyaUserHandler> _logger;

        public GigyaUserHandler(GigyaRestApiClient restApiClient, ILogger<GigyaUserHandler> logger)
        {
            _restApiClient = restApiClient;
            _logger = logger;
        }

        public async Task<LoginResult<object>> OnSuccessLoginAsync(string did)
        {
            var loginResponse = await _restApiClient.NotifyLogin(did, "browser");

            if (loginResponse.SessionInfo == null || loginResponse.ErrorCode != 0)
                return new LoginResult<object>
                {
                    HttpCode = (int) HttpStatusCode.Unauthorized,
                    Data = new
                    {
                        status = false,
                        errorMessage = $"Gigya: {loginResponse.ErrorCode}:{loginResponse.ErrorMessage}"
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

        public async Task UpdateProfileAsync(IUserProfileFormContext<GigyaUserProfile> context)
        {
            var getAccountMessage = await _restApiClient.GetUserInfoByUid(context.DID);

            if (getAccountMessage.ErrorCode == 0)
            {
                if (getAccountMessage.Data == null || !getAccountMessage.Data.ContainsKey("pubKey"))
                    throw new Exception($"Found gigya user uid={context.DID} without pubKey");

                var key = getAccountMessage.Data["pubKey"].ToString();

                if (key != context.PublicKey)
                    throw new Exception("Public key doesn't match gigya user key");

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
            {
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {getAccountMessage.GetFailureMessage()}");
            }

            var loginResponse = await _restApiClient.NotifyLogin(context.DID);

            if (loginResponse.ErrorCode != 0)
            {
                throw new Exception(
                    $"Gigya.notifyLogin error -> {loginResponse.GetFailureMessage()}");
            }

            var setAccountPublicKeyMessage =
                await _restApiClient.SetAccountInfo(context.DID, data: new {pubKey = context.PublicKey});

            if (setAccountPublicKeyMessage.ErrorCode != 0)
            {
                throw new Exception(
                    $"Gigya.setAccountInfo with public key error -> {setAccountPublicKeyMessage.GetFailureMessage()}");
            }

            var setAccountMessage = await _restApiClient.SetAccountInfo(context.DID, context.Profile);

            if (setAccountMessage.ErrorCode == 403043)
            {
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