using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaUserHandler : IUserHandler<GigyaUserProfile>
    {
        private readonly GigyaRestApiClient _restApiClient;

        public GigyaUserHandler(GigyaRestApiClient restApiClient)
        {
            _restApiClient = restApiClient;
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
                    throw new Exception("Found gigya user without pubKey");

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

                    await Console.Error.WriteLineAsync(
                        $"did: {context.DID}{Environment.NewLine}" +
                        $"profile: {exProfileSerializedFields}{Environment.NewLine}" +
                        $"Gigya.setAccountInfo for EXISTING user failed with code {setAccountResponse.ErrorCode} : {setAccountResponse.ErrorMessage}");

                    context.SetGeneralError($"{setAccountResponse.ErrorCode}: {setAccountResponse.ErrorMessage}");
                    // throw new Exception(
                    //     $"Gigya.setAccountInfo for EXISTING user failed with code {setAccountResponse.ErrorCode} : {setAccountResponse.ErrorMessage}");
                }

                return;
            }

            // new user
            if (getAccountMessage.ErrorCode != 403005)
            {
                await Console.Error.WriteLineAsync(
                    $"Gigya.getAccountInfo error with code {getAccountMessage.ErrorCode} : {getAccountMessage.ErrorMessage}");
                context.SetGeneralError($"{getAccountMessage.ErrorCode}: {getAccountMessage.ErrorMessage}");
                // throw new Exception(
                //     $"Gigya.getAccountInfo error with code {content.ErrorCode} : {content.ErrorMessage}");
            }

            var loginResponse = await _restApiClient.NotifyLogin(context.DID);

            if (loginResponse.ErrorCode != 0)
                await Console.Out.WriteLineAsync(
                    $"Gigya.notifyLogin error -> {loginResponse.ErrorCode}: {loginResponse.ErrorMessage}");

            var setAccountPublicKeyMessage =
                await _restApiClient.SetAccountInfo(context.DID, data: new {pubKey = context.PublicKey});
            await Console.Out.WriteLineAsync(JsonSerializer.Serialize(setAccountPublicKeyMessage));

            // if (setAccountPublicKeyMessage.ErrorCode > 0)
            //     throw new Exception(
            //         $"Gigya.setAccountInfo (public key) for NEW user failed with code {setAccountPublicKeyMessage.ErrorCode} : {setAccountPublicKeyMessage.ErrorMessage}");

            var setAccountMessage = await _restApiClient.SetAccountInfo(context.DID, context.Profile);

            if (setAccountMessage.ErrorCode == 403043)
            {
                context.SetError(x => x.Email, setAccountMessage.ErrorMessage);
                return;
            }

            if (setAccountMessage.ErrorCode > 0)
            {
                var profileSerializedFields = JsonSerializer.Serialize(context.Profile, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                });

                await Console.Error.WriteLineAsync(
                    $"did: {context.DID}{Environment.NewLine}" +
                    $"profile: {profileSerializedFields}{Environment.NewLine}" +
                    $"Gigya.setAccountInfo (profile) for NEW user failed with code {setAccountMessage.ErrorCode} : {setAccountMessage.ErrorMessage}");

                context.SetGeneralError($"{setAccountMessage.ErrorCode}: {setAccountMessage.ErrorMessage}");
                // throw new Exception(
                //     $"Gigya.setAccountInfo (profile) for NEW user failed with code {setAccountMessage.ErrorCode} : {setAccountMessage.ErrorMessage}");
            }
        }
    }
}