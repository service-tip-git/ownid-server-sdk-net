using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Extensions;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Link
{
    public class GetAccountLinkDataMiddleware : BaseMiddleware
    {
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;

        public GetAccountLinkDataMiddleware(RequestDelegate next, IAccountLinkHandlerAdapter linkHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<GetAccountLinkDataMiddleware> logger) : base(next, coreConfiguration,
            cacheStore, localizationService, logger)
        {
            _linkHandlerAdapter = linkHandlerAdapter;
        }

        protected override async Task Execute(HttpContext context)
        {
            if (TryGetRequestIdentity(context, out var requestIdentity))
            {
                var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
                if (OwnIdProvider.IsContextFormatValid(requestIdentity.Context) && cacheItem != null &&
                    cacheItem.IsValidForLink)
                {
                    var profile = await _linkHandlerAdapter.GetUserProfileAsync(cacheItem.DID);
                    var culture = GetRequestCulture(context);
                    await OwnIdProvider.SetRequestTokenAsync(requestIdentity.Context, requestIdentity.RequestToken);

                    var tokenData = OwnIdProvider.GenerateProfileDataJwt(requestIdentity.Context,
                        cacheItem.ChallengeType, cacheItem.DID,
                        profile, culture.Name);

                    await OwnIdProvider.SetResponseTokenAsync(cacheItem.Context, tokenData.Hash.GetUrlEncodeString());

                    await Json(context, new JwtContainer
                    {
                        Jwt = tokenData.Jwt
                    }, StatusCodes.Status200OK);

                    return;
                }
            }

            Logger.LogDebug("Failed request identity validation or cache item doesn't exist");
            NotFound(context.Response);
        }
    }
}