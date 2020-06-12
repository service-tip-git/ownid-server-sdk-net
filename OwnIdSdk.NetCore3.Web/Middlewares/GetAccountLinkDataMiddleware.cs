using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetAccountLinkDataMiddleware : BaseMiddleware
    {
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;

        public GetAccountLinkDataMiddleware(RequestDelegate next, IAccountLinkHandlerAdapter linkHandlerAdapter, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService) : base(next, coreConfiguration,
            cacheStore, localizationService)
        {
            _linkHandlerAdapter = linkHandlerAdapter;
        }

        protected override async Task Execute(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(challengeContext);

            if (string.IsNullOrEmpty(challengeContext) || !OwnIdProvider.IsContextFormatValid(challengeContext) ||
                cacheItem == null)
            {
                NotFound(context.Response);
                return;
            }

            var profile = await _linkHandlerAdapter.GetUserProfileAsync(cacheItem.DID);
            var culture = GetRequestCulture(context);

            await Json(context, new JwtContainer
            {
                Jwt = OwnIdProvider.GenerateLinkAccountJwt(challengeContext, cacheItem.ChallengeType, cacheItem.DID,
                    profile, culture.Name)
            }, StatusCodes.Status200OK);
        }
    }
}