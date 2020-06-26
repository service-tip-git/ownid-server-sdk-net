using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GetAccountLinkDataMiddleware : BaseMiddleware
    {
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly ILogger<GetAccountLinkDataMiddleware> _logger;

        public GetAccountLinkDataMiddleware(RequestDelegate next, IAccountLinkHandlerAdapter linkHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<GetAccountLinkDataMiddleware> logger) : base(next, coreConfiguration,
            cacheStore, localizationService, logger)
        {
            _linkHandlerAdapter = linkHandlerAdapter;
            _logger = logger;
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

                    var tokenData = OwnIdProvider.GenerateLinkAccountJwt(requestIdentity.Context,
                        cacheItem.ChallengeType, cacheItem.DID,
                        profile, culture.Name);

                    await OwnIdProvider.SetResponseTokenAsync(cacheItem.Context, tokenData.Hash);

                    await Json(context, new JwtContainer
                    {
                        Jwt = tokenData.Jwt
                    }, StatusCodes.Status200OK);

                    return;
                }
            }

            _logger.LogDebug("Failed request identity validation or cache item doesn't exist");
            NotFound(context.Response);
        }
    }
}