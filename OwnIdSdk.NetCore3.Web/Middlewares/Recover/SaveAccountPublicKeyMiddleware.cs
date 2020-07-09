using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Exceptions;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.FlowEntries.RequestHandling;

namespace OwnIdSdk.NetCore3.Web.Middlewares.Recover
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class SaveAccountPublicKeyMiddleware : BaseMiddleware
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;
        private readonly FlowController _flowControl;

        public SaveAccountPublicKeyMiddleware(
            RequestDelegate next
            , IOwnIdCoreConfiguration coreConfiguration
            , ICacheStore cacheStore
            , ILocalizationService localizationService
            , IAccountRecoveryHandler accountRecoveryHandler
            , ILogger<SaveAccountPublicKeyMiddleware> logger
            , FlowController flowControl
        ) : base(next, coreConfiguration, cacheStore, localizationService, logger)
        {
            _accountRecoveryHandler = accountRecoveryHandler;
            _flowControl = flowControl;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var cacheItem = await GetRequestRelatedCacheItemAsync();

            if (!cacheItem.IsValidForRecover)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Recover challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            ValidateCacheItemTokens(cacheItem);

            var userData = await GetRequestJwtDataAsync<UserProfileData>(httpContext);

            await _accountRecoveryHandler.OnRecoverAsync(userData);

            await OwnIdProvider.FinishAuthFlowSessionAsync(RequestIdentity.Context, userData.DID);

            var jwt = OwnIdProvider.GenerateFinalStepJwt(cacheItem.Context,
                _flowControl.GetNextStep(cacheItem, StepType.Recover), GetRequestCulture(httpContext).Name);

            await Json(httpContext, new JwtContainer(jwt), StatusCodes.Status200OK);
        }
    }
}