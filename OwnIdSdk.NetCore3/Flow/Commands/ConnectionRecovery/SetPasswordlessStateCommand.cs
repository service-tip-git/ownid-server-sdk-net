using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.ConnectionRecovery;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.ConnectionRecovery
{
    public class SetPasswordlessStateCommand
    {
        private readonly ICacheItemService _cacheItemService;

        public SetPasswordlessStateCommand(ICacheItemService cacheItemService)
        {
            _cacheItemService = cacheItemService;
        }

        public async Task<PasswordlessStateResponse> ExecuteAsync(string context, SetPasswordlessStateRequest request)
        {
            string recoveryToken = null;

            var encryptionToken = string.IsNullOrWhiteSpace(request.EncryptionToken)
                ? Guid.NewGuid().ToString("N")
                : request.EncryptionToken;

            if (request.RequiresRecovery)
            {
                recoveryToken = string.IsNullOrWhiteSpace(request.RecoveryToken)
                    ? Guid.NewGuid().ToString("n")
                    : request.RecoveryToken;
            }

            await _cacheItemService.SetPasswordlessStateAsync(context, encryptionToken, recoveryToken);
            
            return new PasswordlessStateResponse
            {
                RecoveryToken = recoveryToken,
                EncryptionToken = encryptionToken
            };
        }
    }
}