using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.AccountRecovery;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts.AccountRecovery;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    /// <summary>
    /// Gigya specific implementation of <see cref="IAccountRecoveryHandler"/>
    /// </summary>
    public class GigyaAccountRecoveryHandler<TProfile> : IAccountRecoveryHandler where TProfile : class, IGigyaUserProfile
    {
        private readonly GigyaRestApiClient<TProfile> _apiClient;

        public GigyaAccountRecoveryHandler(GigyaRestApiClient<TProfile> apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<AccountRecoveryResult> RecoverAsync(string accountRecoveryPayload)
        {
            var payload = JsonSerializer.Deserialize<GigyaAccountRecoveryPayload>(accountRecoveryPayload);
            var newPassword = Guid.NewGuid().ToString("N");

            var resetPasswordResponse = await _apiClient.ResetPasswordAsync(payload.ResetToken, newPassword);
            if (resetPasswordResponse.ErrorCode != 0)
            {
                throw new Exception(
                    $"Gigya.resetPassword error -> {resetPasswordResponse.GetFailureMessage()}");
            }

            var accountInfo = await _apiClient.GetUserInfoByUid(resetPasswordResponse.UID);
            if (accountInfo.ErrorCode != 0)
            {
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.GetFailureMessage()}");
            }

            return new AccountRecoveryResult
            {
                DID = resetPasswordResponse.UID,
                Profile = accountInfo.Profile
            };
        }

        public async Task OnRecoverAsync(UserProfileData userData)
        {
            var responseMessage = await _apiClient.SetAccountInfo<TProfile>(userData.DID, data: new AccountData(userData.PublicKey));

            if (responseMessage.ErrorCode != 0)
            {
                throw new Exception($"Gigya.setAccountInfo error -> {responseMessage.GetFailureMessage()}");
            }
        }
    }
}