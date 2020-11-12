using System;
using System.Linq;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.AccountRecovery;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Web.Gigya.ApiClient;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts.AccountRecovery;
using OwnIdSdk.NetCore3.Web.Gigya.Contracts.Accounts;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    /// <summary>
    ///     Gigya specific implementation of <see cref="IAccountRecoveryHandler" />
    /// </summary>
    public class GigyaAccountRecoveryHandler<TProfile> : IAccountRecoveryHandler
        where TProfile : class, IGigyaUserProfile
    {
        private readonly GigyaRestApiClient<TProfile> _apiClient;

        public GigyaAccountRecoveryHandler(GigyaRestApiClient<TProfile> apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<AccountRecoveryResult> RecoverAsync(string accountRecoveryPayload)
        {
            var payload = OwnIdSerializer.Deserialize<GigyaAccountRecoveryPayload>(accountRecoveryPayload);
            var newPassword = Guid.NewGuid().ToString("N");

            var resetPasswordResponse = await _apiClient.ResetPasswordAsync(payload.ResetToken, newPassword);

            if (resetPasswordResponse.ErrorCode != 0)
                throw new OwnIdException(resetPasswordResponse.ErrorDetails);

            return new AccountRecoveryResult
            {
                DID = resetPasswordResponse.UID
            };
        }

        public async Task OnRecoverAsync(string did, OwnIdConnection connection)
        {
            var gigyaOwnIdConnection = new GigyaOwnIdConnection(connection);

            var responseMessage =
                await _apiClient.SetAccountInfo<TProfile>(did, data: new AccountData(gigyaOwnIdConnection));

            if (responseMessage.ErrorCode != 0)
                throw new Exception($"Gigya.setAccountInfo error -> {responseMessage.GetFailureMessage()}");
        }

        public async Task RemoveConnectionsAsync(string publicKey)
        {
            var did = await _apiClient.SearchForDid(publicKey);
            
            if (string.IsNullOrEmpty(did))
                return;
            
            var profile = await _apiClient.GetUserInfoByUid(did);
            var connectionToRemove = profile.Data.Connections.Single(c => c.PublicKey == publicKey);
            profile.Data.Connections.Remove(connectionToRemove);

            await _apiClient.SetAccountInfo<TProfile>(did, profile.Profile, profile.Data);
        }
    }
}