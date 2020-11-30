using System.Threading.Tasks;
using OwnID.Services;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts;

namespace OwnID.Flow.Commands.Approval
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