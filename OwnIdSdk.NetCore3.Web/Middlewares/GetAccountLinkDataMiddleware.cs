using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            if (TryGetRequestIdentity(context, out var requestIdentity))
            {
                var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
                if (OwnIdProvider.IsContextFormatValid(requestIdentity.Context) && cacheItem != null)
                {
                    var profile = await _linkHandlerAdapter.GetUserProfileAsync(cacheItem.DID);
                    var culture = GetRequestCulture(context);
                    await OwnIdProvider.SetRequestTokenAsync(requestIdentity.Context, requestIdentity.RequestToken);

                    await Json(context, new JwtContainer
                    {
                        Jwt = OwnIdProvider.GenerateLinkAccountJwt(requestIdentity.Context, cacheItem.ChallengeType, cacheItem.DID,
                            profile, culture.Name)
                    }, StatusCodes.Status200OK);
                    
                    return;
                }
            }
            
            NotFound(context.Response);
        }
    }
}