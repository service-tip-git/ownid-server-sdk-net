using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.AccountRecovery;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions
{
    /// <summary>
    ///     Define a interface for supporting account recovery process
    /// </summary>
    public interface IAccountRecoveryHandler
    {
        /// <summary>
        ///     Recover account access
        /// </summary>
        /// <param name="accountRecoveryPayload">account recovery payload</param>
        /// <returns>
        ///     A task that represents the asynchronous recover operation.
        ///     The task result contains <see cref="AccountRecoveryResult" />
        /// </returns>
        Task<AccountRecoveryResult> RecoverAsync(string accountRecoveryPayload);

        /// <summary>
        ///     Operation to be run after successful account recover
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous recover operation.
        /// </returns>
        Task OnRecoverAsync(string did, string publicKey, string fido2UserId = null, string fido2CredentialId = null,
            uint? fido2SignatureCounter = null);
    }
}