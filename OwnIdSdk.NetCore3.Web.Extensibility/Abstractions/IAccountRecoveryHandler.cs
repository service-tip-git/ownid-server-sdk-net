using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Contracts.AccountRecovery;
using OwnIdSdk.NetCore3.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    /// <summary>
    /// Define a interface for supporting account recovery process
    /// </summary>
    public interface IAccountRecoveryHandler
    {
        /// <summary>
        /// Recover account access
        /// </summary>
        /// <param name="accountRecoveryPayload">account recovery payload</param>
        /// <returns>
        /// A task that represents the asynchronous recover operation.
        /// The task result contains <see cref="AccountRecoveryResult"/>
        /// </returns>
        Task<AccountRecoveryResult> RecoverAsync(string accountRecoveryPayload);

        /// <summary>
        /// Operation to be run after successful account recover
        /// </summary>
        /// <param name="userData">user data</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        Task OnRecoverAsync(UserProfileData userData);
    }
}