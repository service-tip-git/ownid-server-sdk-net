using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeStatusMiddleware : BaseMiddleware
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public GetChallengeStatusMiddleware(RequestDelegate next, IUserHandlerAdapter userHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration, ICacheStore cacheStore,
            ILocalizationService localizationService) : base(next, coreConfiguration, cacheStore,
            localizationService)
        {
            _userHandlerAdapter = userHandlerAdapter;
        }

        protected override async Task Execute(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            if (string.IsNullOrEmpty(challengeContext) || !OwnIdProvider.IsContextFormatValid(challengeContext))
            {
                NotFound(context.Response);
                return;
            }

            var request = await JsonSerializer.DeserializeAsync<GetStatusRequest>(context.Request.Body);

            if (string.IsNullOrEmpty(request.Nonce))
            {
                NotFound(context.Response);
                return;
            }

            var didResult = await OwnIdProvider.GetDIDAsync(challengeContext, request.Nonce);

            if (didResult.isSuccess)
            {
                await OwnIdProvider.RemoveContextAsync(challengeContext);

                var result = await _userHandlerAdapter.OnSuccessLoginAsync(didResult.did);

                await Json(context, result.Data, result.HttpCode, false);
                return;
            }

            await Json(context, new GetStatusResponse {IsSuccess = didResult.isSuccess}, StatusCodes.Status200OK,
                false);
        }
    }
}