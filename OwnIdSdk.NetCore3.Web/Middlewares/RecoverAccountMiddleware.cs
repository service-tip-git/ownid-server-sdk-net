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
        private readonly ILogger<RecoverAccountMiddleware> _logger;

        public RecoverAccountMiddleware(
            RequestDelegate next
            , IOwnIdCoreConfiguration coreConfiguration
            , ICacheStore cacheStore
            , ILocalizationService localizationService
            , IAccountRecoveryHandler accountRecoveryHandler
            , ILogger<RecoverAccountMiddleware> logger
            ) : base(next, coreConfiguration, cacheStore, localizationService, logger)
        {
            _accountRecoveryHandler = accountRecoveryHandler;
            _logger = logger;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            // TODO: add request/response token validation
            if (!TryGetRequestIdentity(httpContext, out var requestIdentity)
                || !OwnIdProvider.IsContextFormatValid(requestIdentity.Context))
            {
                _logger.LogDebug("Failed request identity validation");
                NotFound(httpContext.Response);
                return;
            }
            
            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
            if (cacheItem == null
                || cacheItem.IsFinished
                || cacheItem.ChallengeType != ChallengeType.Recover
            )
            {
                _logger.LogError("No such cache item or incorrect request/response token");
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