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
        }

        protected override async Task Execute(HttpContext context)
        {
            if (!TryGetRequestIdentity(context, out var requestIdentity) ||
                !OwnIdProvider.IsContextFormatValid(requestIdentity.Context))
            {
                NotFound(context.Response);
                return;
            }

            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
            if (cacheItem == null || cacheItem.RequestToken != requestIdentity.RequestToken)
            {
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