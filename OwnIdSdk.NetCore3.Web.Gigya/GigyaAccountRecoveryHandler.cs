using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.AccountRecovery;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts.AccountRecovery;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    /// <summary>
    /// Gigya specific implementation of <see cref="IAccountRecoveryHandler"/>
    /// </summary>
    public class GigyaAccountRecoveryHandler : IAccountRecoveryHandler
    {
        private readonly GigyaRestApiClient _apiClient;

        public GigyaAccountRecoveryHandler(GigyaRestApiClient apiClient)
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
                    $"Gigya.resetPassword error -> {resetPasswordResponse.ErrorCode}: {resetPasswordResponse.ErrorMessage}");
            }

            var accountInfo = await _apiClient.GetUserInfoByUid(resetPasswordResponse.UID);
            if (accountInfo.ErrorCode != 0)
            {
                throw new Exception(
                    $"Gigya.getAccountInfo error -> {accountInfo.ErrorCode}: {accountInfo.ErrorMessage}");
            }

            //
            // TODO: use reflection to fill fields
            //
            return new AccountRecoveryResult
            {
                DID = resetPasswordResponse.UID,
                Profile = new GigyaUserProfile
                {
                    Email = TryGetValue(accountInfo.Profile, "email"),
                    FirstName = TryGetValue(accountInfo.Profile, "firstName"),
                    LastName = TryGetValue(accountInfo.Profile, "lastName"),
                    Nickname = TryGetValue(accountInfo.Profile, "nickName")
                }
            };
        }

        public async Task OnRecoverAsync(UserProfileData userData)
        {
            var responseMessage = await _apiClient.SetAccountInfo(userData.DID, data: new { pubKey = userData.PublicKey });

            if (responseMessage.ErrorCode != 0)
            {
                throw new Exception($"Gigya.setAccountInfo error -> {responseMessage.ErrorCode}: {responseMessage.ErrorMessage}");
            }
        }

        private string TryGetValue(Dictionary<string, string> data, string key)
        {
            return data.TryGetValue(key, out var value) ? value : null;
        }
    }
}