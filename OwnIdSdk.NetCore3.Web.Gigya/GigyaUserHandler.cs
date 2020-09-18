using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaUserHandler<TProfile> : IUserHandler<TProfile> where TProfile : class, IGigyaUserProfile
    {
        private readonly GigyaConfiguration _configuration;
        private readonly ILogger<GigyaUserHandler<TProfile>> _logger;
        private readonly GigyaRestApiClient<TProfile> _restApiClient;

        public GigyaUserHandler(GigyaRestApiClient<TProfile> restApiClient, GigyaConfiguration configuration,
            ILogger<GigyaUserHandler<TProfile>> logger)
        {
            _restApiClient = restApiClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResult<object>> OnSuccessLoginAsync(string did, string publicKey)
        {
            return await OnSuccessLoginByPublicKeyAsync(publicKey);
        }

        public async Task<AuthResult<object>> OnSuccessLoginByPublicKeyAsync(string publicKey)
        {
            var did = await _restApiClient.SearchForDid(publicKey);

            if (string.IsNullOrEmpty(did))
                return new AuthResult<object>("Can not find user in Gigya search result with provided public key");

            return await OnSuccessLoginInternalAsync(did);
        }

        private async Task<AuthResult<object>> OnSuccessLoginInternalAsync(string did)
        {
            if (_configuration.LoginType == GigyaLoginType.Session)
            {
                var loginResponse = await _restApiClient.NotifyLogin(did, "browser");

                if (loginResponse.SessionInfo == null || loginResponse.ErrorCode != 0)
                    return new AuthResult<object>($"Gigya: {loginResponse.GetFailureMessage()}");

                return new AuthResult<object>(new
                {
                    sessionInfo = loginResponse.SessionInfo,
                    identities = loginResponse.Identities.FirstOrDefault()
                });
            }

            var jwtResponse = await _restApiClient.GetJwt(did);

            if (jwtResponse.IdToken == null || jwtResponse.ErrorCode != 0)
                return new AuthResult<object>($"Gigya: {jwtResponse.GetFailureMessage()}");

            return new AuthResult<object>(new
            {
                idToken = jwtResponse.IdToken
            });
        }

        public async Task<AuthResult<object>> OnSuccessLoginByFido2Async(string fido2CredentialId,
            uint fido2SignCounter)
        {
            var user = await _restApiClient.SearchByFido2CredentialId(fido2CredentialId);
            if (user == null)
                return new AuthResult<object>("Can not find user in Gigya with provided fido2 user id");

            var connectionToUpdate = user.Data.Connections.First(x => x.Fido2CredentialId == fido2CredentialId);

            // Update signature counter
            connectionToUpdate.Fido2SignatureCounter = fido2SignCounter;

            var setAccountResponse = await _restApiClient.SetAccountInfo(user.UID, (TProfile) null, user.Data);
            if (setAccountResponse.ErrorCode > 0)
            {
                throw new Exception(
                    $"did: {user.UID} " +
                    $"Gigya.setAccountInfo for EXISTING user error -> {setAccountResponse.GetFailureMessage()}");
            }

            return await OnSuccessLoginInternalAsync(user.UID);
        }

        public async Task<IdentitiesCheckResult> CheckUserIdentitiesAsync(string did, string publicKey)
        {
            var getAccountMessage = await _restApiClient.GetUserInfoByUid(did);

            if (getAccountMessage.ErrorCode == 403005)
                return IdentitiesCheckResult.UserNotFound;

            if (getAccountMessage.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.notifyLogin error -> {getAccountMessage.GetFailureMessage()}");

            if (getAccountMessage.Data == null || !getAccountMessage.Data.Connections.Any())
            {
                _logger.LogError($"Found gigya user uid={did} without pubKey");
                return IdentitiesCheckResult.WrongPublicKey;
            }

            if (getAccountMessage.Data.Connections.All(x => x.PublicKey != publicKey))
            {
                _logger.LogError("Public key doesn't match any of gigya user keys");
                return IdentitiesCheckResult.WrongPublicKey;
            }

            return IdentitiesCheckResult.UserExists;
        }

        public async Task<bool> IsUserExists(string publicKey)
        {
            var did = await _restApiClient.SearchForDid(publicKey);
            return !string.IsNullOrWhiteSpace(did);
        }

        public async Task<bool> IsFido2UserExists(string fido2CredentialId)
        {
            var user = await _restApiClient.SearchByFido2CredentialId(fido2CredentialId);
            return user != null;
        }

        public async Task<Fido2Info> FindFido2Info(string fido2CredentialId)
        {
            var user = await _restApiClient.SearchByFido2CredentialId(fido2CredentialId);
            if (user == null)
                return null;

            var connection = user.Data.Connections.First(c => c.Fido2CredentialId == fido2CredentialId);

            if (connection.Fido2SignatureCounter == null)
                throw new Exception(
                    $"connection with fido2 credential id '{fido2CredentialId}' doesn't have signature count");

            return new Fido2Info
            {
                UserId = user.UID,
                PublicKey = connection.PublicKey,
                SignatureCounter = connection.Fido2SignatureCounter.Value,
                CredentialId = connection.Fido2CredentialId
            };
        }

        public async Task CreateProfileAsync(IUserProfileFormContext<TProfile> context)
        {
            var loginResponse = await _restApiClient.NotifyLogin(context.DID);

            if (loginResponse.ErrorCode != 0)
                throw new Exception(
                    $"Gigya.notifyLogin error -> {loginResponse.GetFailureMessage()}");

            var setAccountMessage =
                await _restApiClient.SetAccountInfo(context.DID, context.Profile, new AccountData(context.PublicKey));

            if (setAccountMessage.ErrorCode > 0)
            {
                _logger.LogError($"did: {context.DID} " +
                                 $"Gigya.setAccountInfo (profile) for NEW user error -> {setAccountMessage.GetFailureMessage()}");

                var removeUserResult = await _restApiClient.DeleteAccountAsync(context.DID);

                if (removeUserResult.ErrorCode != 0)
                    throw new Exception(
                        $"Gigya.deleteAccount with uid={context.DID} error -> {removeUserResult.GetFailureMessage()}");

                SetErrorsToContext(context, setAccountMessage);
            }
        }

        public async Task UpdateProfileAsync(IUserProfileFormContext<TProfile> context)
        {
            var setAccountResponse = await _restApiClient.SetAccountInfo(context.DID, context.Profile);

            if (setAccountResponse.ErrorCode > 0)
            {
                _logger.LogError(
                    $"did: {context.DID} " +
                    $"Gigya.setAccountInfo for EXISTING user error -> {setAccountResponse.GetFailureMessage()}");

                SetErrorsToContext(context, setAccountResponse);
            }
        }

        private static void SetErrorsToContext(IUserProfileFormContext<TProfile> context, BaseGigyaResponse response)
        {
            if (response.ValidationErrors != null && response.ValidationErrors.Any())
            {
                foreach (var validationError in response.ValidationErrors)
                {
                    //
                    // TODO: find better way to map field name to profile property
                    //
                    switch (validationError.FieldName)
                    {
                        case "profile.email":
                            context.SetError(x => x.Email, validationError.Message);
                            break;
                        default:
                            context.SetGeneralError(validationError.Message);
                            break;
                    }
                }
            }
            else
            {
                context.SetGeneralError(response.UserFriendlyFailureMessage);
            }
        }
    }
}