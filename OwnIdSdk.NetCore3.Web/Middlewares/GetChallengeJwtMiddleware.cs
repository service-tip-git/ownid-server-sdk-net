using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeJwtMiddleware : BaseMiddleware
    {
        public GetChallengeJwtMiddleware(RequestDelegate next, IChallengeHandler challengeHandler,
            ICacheStore cacheStore, IOptions<OwnIdConfiguration> providerConfiguration) : base(next, challengeHandler,
            cacheStore, providerConfiguration)
        {
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

            // TODO: do we need to check if context exists
            await Ok(context.Response, new JwtContainer
            {
                Jwt = Provider.GenerateChallengeJwt(challengeContext)
            });
        }
    }
}