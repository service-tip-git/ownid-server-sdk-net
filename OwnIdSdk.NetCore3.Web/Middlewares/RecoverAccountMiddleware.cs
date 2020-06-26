using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class RecoverAccountMiddleware : BaseMiddleware
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;

        public RecoverAccountMiddleware(
            RequestDelegate next
            , IOwnIdCoreConfiguration coreConfiguration
            , ICacheStore cacheStore
            , ILocalizationService localizationService
            , IAccountRecoveryHandler accountRecoveryHandler
            , ILogger logger
            ) : base(next, coreConfiguration, cacheStore, localizationService, logger)
        {
            _accountRecoveryHandler = accountRecoveryHandler;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            if (!TryGetRequestIdentity(httpContext, out var requestIdentity)
                || !OwnIdProvider.IsContextFormatValid(requestIdentity.Context))
            {
                NotFound(httpContext.Response);
                return;
            }
            
            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
            if (cacheItem == null
                || cacheItem.IsFinished
                || cacheItem.ChallengeType != ChallengeType.Recover
            )
            {
                NotFound(httpContext.Response);
                return;
            }

            // Recover access and get user profile
            var recoverResult = await _accountRecoveryHandler.RecoverAsync(cacheItem.Payload);
            
            var culture = GetRequestCulture(httpContext);
            await OwnIdProvider.SetRequestTokenAsync(requestIdentity.Context, requestIdentity.RequestToken);

            var jwt = OwnIdProvider.GenerateProfileDataJwt(requestIdentity.Context, cacheItem.ChallengeType,
                recoverResult.DID,
                recoverResult.Profile, culture.Name);
            await Json(httpContext, new JwtContainer
            {
                Jwt = jwt.Jwt  
            }, StatusCodes.Status200OK);
        }
    }
}