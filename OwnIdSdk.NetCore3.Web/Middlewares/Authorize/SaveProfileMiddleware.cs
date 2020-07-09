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
    public class SaveProfileMiddleware : BaseMiddleware
    {
        private readonly FlowController _flowController;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public SaveProfileMiddleware(RequestDelegate next, IUserHandlerAdapter userHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration, ICacheStore cacheStore,
            ILocalizationService localizationService, ILogger<SaveProfileMiddleware> logger,
            FlowController flowController) : base(next,
            coreConfiguration, cacheStore,
            localizationService, logger)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _flowController = flowController;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var cacheItem = await GetRequestRelatedCacheItemAsync();

            if (!cacheItem.IsValidForLoginRegister)
                throw new RequestValidationException(
                    "Cache item should be not be Finished with Login or Register challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            ValidateCacheItemTokens(cacheItem);

            var userData = await GetRequestJwtDataAsync<UserProfileData>(httpContext);

            var formContext = _userHandlerAdapter.CreateUserDefinedContext(userData, LocalizationService);

            formContext.Validate();

            if (formContext.HasErrors)
                throw new BusinessValidationException(formContext);

            await _userHandlerAdapter.UpdateProfileAsync(formContext);

            if (!formContext.HasErrors)
            {
                var locale = GetRequestCulture(httpContext).Name;
                await OwnIdProvider.FinishAuthFlowSessionAsync(RequestIdentity.Context, userData.DID);
                var jwt = OwnIdProvider.GenerateFinalStepJwt(cacheItem.Context,
                    _flowController.GetNextStep(cacheItem, StepType.Authorize), locale);

                await Json(httpContext, new JwtContainer(jwt), StatusCodes.Status200OK);
            }
            else
            {
                throw new BusinessValidationException(formContext);
            }
        }
    }
}