using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeStatusMiddleware : BaseMiddleware
    {
        private readonly IChallengeHandlerAdapter _challengeHandlerAdapter;

        public GetChallengeStatusMiddleware(RequestDelegate next, IChallengeHandlerAdapter challengeHandlerAdapter,
            IOptions<OwnIdConfiguration> providerConfiguration, ICacheStore cacheStore,
            ILocalizationService localizationService) : base(next, providerConfiguration, cacheStore,
            localizationService)
        {
            _challengeHandlerAdapter = challengeHandlerAdapter;
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            if (string.IsNullOrEmpty(challengeContext) || !Provider.IsContextFormatValid(challengeContext))
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

            var didResult = await Provider.GetDIDAsync(challengeContext, request.Nonce);

            if (didResult.isSuccess)
            {
                await Provider.RemoveContextAsync(challengeContext);

                var result = await _challengeHandlerAdapter.OnSuccessLoginAsync(didResult.did, context.Response);

                await Json(context, result.Data, result.HttpCode, false);
                return;
            }

            await Json(context, new GetStatusResponse {IsSuccess = didResult.isSuccess}, StatusCodes.Status200OK,
                false);
        }
    }
}