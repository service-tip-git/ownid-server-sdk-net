using System.Threading.Tasks;
using OwnID.Extensibility.Flow.Contracts.Internal;
using OwnID.Flow.Adapters;

namespace OwnID.Commands
{
    public class InternalConnectionRecoveryCommand
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public InternalConnectionRecoveryCommand(IUserHandlerAdapter userHandlerAdapter)
        {
            _userHandlerAdapter = userHandlerAdapter;
        }

        public async Task<ConnectionRecoveryResult<object>> ExecuteAsync(string recoveryToken)
        {
            return await _userHandlerAdapter.GetConnectionRecoveryDataAsync(recoveryToken, true);
        }
    }
}