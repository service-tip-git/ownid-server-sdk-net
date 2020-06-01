using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeJwtMiddleware : BaseMiddleware
    {
        public GetChallengeJwtMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService) : base(next, coreConfiguration,
            cacheStore, localizationService)
        {
        }

        protected override async Task Execute(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            // TODO: do we need to check if context exists
            if (string.IsNullOrEmpty(challengeContext) || !OwnIdProvider.IsContextFormatValid(challengeContext))
            {
                NotFound(context.Response);
                return;
            }

            var culture = GetRequestCulture(context);

            await Json(context, new JwtContainer
            {
                Jwt = OwnIdProvider.GenerateChallengeJwt(challengeContext, culture.Name)
            }, StatusCodes.Status200OK);
        }
    }
}