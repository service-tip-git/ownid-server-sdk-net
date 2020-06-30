using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class SaveAccountPublicKeyMiddleware: BaseMiddleware
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;
        private readonly ILogger<SaveAccountPublicKeyMiddleware> _logger;

        public SaveAccountPublicKeyMiddleware(
            RequestDelegate next
            , IOwnIdCoreConfiguration coreConfiguration
            , ICacheStore cacheStore
            , ILocalizationService localizationService
            , IAccountRecoveryHandler accountRecoveryHandler
            , ILogger<SaveAccountPublicKeyMiddleware> logger
            ) : base(next, coreConfiguration, cacheStore, localizationService, logger)
        {
            _accountRecoveryHandler = accountRecoveryHandler;
            _logger = logger;
        }

        protected override async Task Execute(HttpContext context)
        {
            // TODO: add request/response token validation
            if (!TryGetRequestIdentity(context, out var requestIdentity) ||
                !OwnIdProvider.IsContextFormatValid(requestIdentity.Context))
            {
                _logger.LogError("Failed request identity validation");
                NotFound(context.Response);
                return;
            }

            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
            if (cacheItem == null || cacheItem.RequestToken != requestIdentity.RequestToken)
            {
                _logger.LogError("No such cache item or incorrect request/response token");
                NotFound(context.Response);
                return;
            }
            
            var request = await JsonSerializer.DeserializeAsync<JwtContainer>(context.Request.Body);
            if (string.IsNullOrEmpty(request?.Jwt))
            {
                BadRequest(context.Response);
                return;
            }

            var (jwtContext, userData) = OwnIdProvider.GetDataFromJwt<UserProfileData>(request.Jwt);
            if (jwtContext != requestIdentity.Context)
            {
                BadRequest(context.Response);
                return;
            }

            await _accountRecoveryHandler.OnRecoverAsync(userData);
            
            await OwnIdProvider.FinishAuthFlowSessionAsync(requestIdentity.Context, userData.DID);
            
            Ok(context.Response);
        }
    }
}