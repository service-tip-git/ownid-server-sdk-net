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

namespace OwnIdSdk.NetCore3.Web.Middlewares.Authorize
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken |
                       BaseRequestFields.ResponseToken)]
    public class SavePartialProfileMiddleware : BaseMiddleware
    {
        private readonly FlowController _flowController;

        public SavePartialProfileMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<SavePartialProfileMiddleware> logger, FlowController flowController,
            IUserHandlerAdapter userHandlerAdapter) : base(next,
            coreConfiguration, cacheStore, localizationService, logger)
        {
            _flowController = flowController;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var cacheItem = await GetRequestRelatedCacheItemAsync();

            if (!cacheItem.IsValidForLoginRegister)
                throw new RequestValidationException(
                    "Cache item should be not be Finished with PARTIAL Login or Register challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            ValidateCacheItemTokens(cacheItem);

            var userData = await GetRequestJwtDataAsync<UserPartialData>(httpContext);

            if (string.IsNullOrEmpty(userData.PublicKey))
                throw new RequestValidationException("No public key was provided for partial flow");

            await OwnIdProvider.SetPublicKeyAsync(cacheItem.Context, userData.PublicKey);

            var locale = GetRequestCulture(httpContext).Name;
            await OwnIdProvider.FinishAuthFlowSessionAsync(RequestIdentity.Context, userData.DID);
            var jwt = OwnIdProvider.GenerateFinalStepJwt(cacheItem.Context,
                _flowController.GetNextStep(cacheItem, StepType.InstantAuthorize), locale);

            await Json(httpContext, new JwtContainer(jwt), StatusCodes.Status200OK);
        }
    }
}