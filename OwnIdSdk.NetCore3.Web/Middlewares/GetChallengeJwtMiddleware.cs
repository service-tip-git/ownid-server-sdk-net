using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeJwtMiddleware : BaseMiddleware
    {
        public GetChallengeJwtMiddleware(RequestDelegate next, IOptions<OwnIdConfiguration> providerConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService) : base(next, providerConfiguration,
            cacheStore, localizationService)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            // TODO: do we need to check if context exists
            if (string.IsNullOrEmpty(challengeContext) || !Provider.IsContextFormatValid(challengeContext))
            {
                NotFound(context.Response);
                return;
            }

            var culture = GetRequestCulture(context);

            await Json(context, new JwtContainer
            {
                Jwt = Provider.GenerateChallengeJwt(challengeContext, culture.Name)
            }, StatusCodes.Status200OK);
        }
    }
}