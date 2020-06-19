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
            if (TryGetRequestIdentity(context, out var requestIdentity))
            {
                var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
                if (OwnIdProvider.IsContextFormatValid(requestIdentity.Context) && cacheItem != null)
                {
                    var culture = GetRequestCulture(context);

                    await OwnIdProvider.SetRequestTokenAsync(requestIdentity.Context, requestIdentity.RequestToken);

                    await Json(context, new JwtContainer
                    {
                        Jwt = OwnIdProvider.GenerateChallengeJwt(requestIdentity.Context, cacheItem.ChallengeType, culture.Name)
                    }, StatusCodes.Status200OK);
                    
                    return;
                }
            }
            
            NotFound(context.Response);
        }
    }
}