using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetChallengeJwtMiddleware : BaseMiddleware
    {
        private readonly ILogger<GetChallengeJwtMiddleware> _logger;

        public GetChallengeJwtMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService, ILogger<GetChallengeJwtMiddleware> logger)
            : base(next, coreConfiguration,
                cacheStore, localizationService, logger)
        {
            _logger = logger;
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

                    var tokenData = OwnIdProvider.GenerateChallengeJwt(requestIdentity.Context, cacheItem.ChallengeType,
                        culture.Name);
                    await OwnIdProvider.SetResponseTokenAsync(cacheItem.Context, tokenData.Hash.GetUrlEncodeString());

                    await Json(context, new JwtContainer
                    {
                        Jwt = tokenData.Jwt
                    }, StatusCodes.Status200OK);

                    return;
                }
            }

            _logger.LogError("Failed request identity validation or cache item doesn't exist");
            NotFound(context.Response);
        }
    }
}