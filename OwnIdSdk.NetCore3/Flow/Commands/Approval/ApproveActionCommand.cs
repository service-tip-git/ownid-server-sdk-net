using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Approval
{
    public class ApproveActionCommand
    {
        private readonly ICacheItemService _cacheItemService;

        public ApproveActionCommand(ICacheItemService cacheItemService)
        {
            _cacheItemService = cacheItemService;
        }

        public async Task ExecuteAsync(ApproveActionRequest request)
        {
            if (string.IsNullOrEmpty(request.Context) || string.IsNullOrEmpty(request.Nonce))
                throw new CommandValidationException("Context and nonce are required");

            await _cacheItemService.SetApprovalResolutionAsync(request.Context, request.Nonce, request.IsApproved);
        }
    }
}