using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeStatusMiddleware : BaseMiddleware
    {
        public GetChallengeStatusMiddleware(RequestDelegate next, IChallengeHandler challengeHandler,
            ICacheStore cacheStore, ProviderConfiguration providerConfiguration) : base(next, challengeHandler,
            cacheStore, providerConfiguration)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            if (string.IsNullOrEmpty(challengeContext) || !_provider.IsContextValid(challengeContext))
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

            var didResult = await _provider.GetDIDAsync(challengeContext, request.Nonce);

            // TODO: call IChallengeHandler

            // TODO: set session/token/auth-cookie
            await Ok(context.Response, new GetStatusResponse{IsSuccess = didResult.isSuccess});
        }
    }
}