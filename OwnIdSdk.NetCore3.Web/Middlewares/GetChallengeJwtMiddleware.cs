using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeJwtMiddleware : BaseMiddleware
    {
        public GetChallengeJwtMiddleware(RequestDelegate next, IChallengeHandler challengeHandler,
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

            await Ok(context.Response, new {jwt = _provider.GenerateChallengeJwt(challengeContext)});
        }
    }
}