using System.Threading.Tasks;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.AccountRecovery;

namespace OwnID.Extensibility.Flow.Abstractions
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
        /// <exception cref="RecoveryTokenExpiredException">Thrown when recovery token expired</exception>
        Task<AccountRecoveryResult> RecoverAsync(string accountRecoveryPayload);

        /// <summary>
        ///     Operation to be run after successful account recover
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous recover operation.
        /// </returns>
        Task OnRecoverAsync(string did, OwnIdConnection connection);

        /// <summary>
        ///     Remove existing connections from existing users
        /// </summary>
        /// <param name="publicKey">public key to be cleared</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        Task RemoveConnectionsAsync(string publicKey);
    }
}