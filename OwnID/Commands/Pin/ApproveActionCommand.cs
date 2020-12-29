using System;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Services;

namespace OwnID.Commands.Pin
{
    public class ApproveActionCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;

        public ApproveActionCommand(ICacheItemRepository cacheItemRepository)
        {
            _cacheItemRepository = cacheItemRepository;
        }

        public async Task ExecuteAsync(ApproveActionRequest request)
        {
            if (string.IsNullOrEmpty(request.Context) || string.IsNullOrEmpty(request.Nonce))
                throw new CommandValidationException("Context and nonce are required");


            await _cacheItemRepository.UpdateAsync(request.Context, item =>
            {
                if (item.Nonce != request.Nonce)
                    throw new ArgumentException(
                        $"Can not find any item with context '{request.Context}' and nonce '{request.Nonce}'");

                if (item.Status != CacheItemStatus.WaitingForApproval)
                    throw new ArgumentException(
                        $"Incorrect status={item.Status.ToString()} for approval '{request.Context}'");

                item.Status = request.IsApproved ? CacheItemStatus.Approved : CacheItemStatus.Declined;
            });
        }
    }
}