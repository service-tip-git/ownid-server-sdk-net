using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeStatusMiddleware : BaseMiddleware
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public GetChallengeStatusMiddleware(
            RequestDelegate next
            , IUserHandlerAdapter userHandlerAdapter
            , IOwnIdCoreConfiguration coreConfiguration
            , ICacheStore cacheStore
            , ILocalizationService localizationService
            , ILogger<GetChallengeStatusMiddleware> logger
        ) : base(next, coreConfiguration, cacheStore, localizationService, logger)
        {
            _userHandlerAdapter = userHandlerAdapter;
        }

        protected override async Task Execute(HttpContext context)
        {
            List<GetStatusRequest> request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<List<GetStatusRequest>>(context.Request.Body);
            }
            catch
            {
                BadRequest(context.Response);
                return;
            }

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

            await Json(context, response, StatusCodes.Status200OK, false);
        }

        private async Task<GetStatusResponse> GetContextStatus(GetStatusRequest requestItem)
        {
            if (string.IsNullOrEmpty(requestItem.Context) || !OwnIdProvider.IsContextFormatValid(requestItem.Context))
                return null;

            if (string.IsNullOrEmpty(requestItem.Nonce))
                return null;

            var cacheItem = await OwnIdProvider.PopFinishedAuthFlowSessionAsync(requestItem.Context, requestItem.Nonce);

            if (cacheItem == null)
                return null;

            var result = new GetStatusResponse
            {
                Status = cacheItem.Status,
                Context = requestItem.Context
            };

            if (cacheItem.SecurityCode != null && cacheItem.Status == CacheItemStatus.WaitingForApproval)
                // TODO: refactor
                result.Payload = new
                {
                    data = new
                    {
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