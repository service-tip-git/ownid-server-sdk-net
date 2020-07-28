using System.Collections.Generic;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class GetStatusCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public GetStatusCommand(IUserHandlerAdapter userHandlerAdapter, ICacheItemService cacheItemService)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
        }

        public async Task<List<GetStatusResponse>> ExecuteAsync(List<GetStatusRequest> request)
        {
            var response = new List<GetStatusResponse>();

            foreach (var statusRequestItem in request)
            {
                var responseItem = await GetContextStatus(statusRequestItem);

                if (responseItem == null)
                    continue;

                if (responseItem.Status == CacheItemStatus.Finished)
                {
                    response = new List<GetStatusResponse>(1) {responseItem};
                    break;
                }

                response.Add(responseItem);
            }

            return response;
        }

        private async Task<GetStatusResponse> GetContextStatus(GetStatusRequest requestItem)
        {
            if (string.IsNullOrEmpty(requestItem.Context) || string.IsNullOrEmpty(requestItem.Nonce))
                return null;

            var cacheItem =
                await _cacheItemService.PopFinishedAuthFlowSessionAsync(requestItem.Context, requestItem.Nonce);

            if (cacheItem == null)
                return null;

            var result = new GetStatusResponse
            {
                Status = cacheItem.Status,
                Context = requestItem.Context
            };

            if (cacheItem.SecurityCode != null && cacheItem.Status == CacheItemStatus.WaitingForApproval)
                // TODO: refactor to entity
                result.Payload = new
                {
                    data = new {
                        pin = cacheItem.SecurityCode
                    }
                };

            if (cacheItem.Status == CacheItemStatus.Finished)
            {
                if (cacheItem.PublicKey == null)
                {
                    result.Payload = await _userHandlerAdapter.OnSuccessLoginAsync(cacheItem.DID);
                }
                else
                {
                    if (cacheItem.ChallengeType == ChallengeType.Login)
                        result.Payload = await _userHandlerAdapter.OnSuccessLoginByPublicKeyAsync(cacheItem.PublicKey);
                    else
                        result.Payload = new
                        {
                            data = new
                            {
                                publicKey = cacheItem.PublicKey
                            }
                        };
                }
            }

            return result;
        }
    }
}