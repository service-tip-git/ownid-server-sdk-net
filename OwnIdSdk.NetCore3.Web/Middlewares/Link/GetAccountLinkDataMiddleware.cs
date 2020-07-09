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

namespace OwnIdSdk.NetCore3.Web.Middlewares.Link
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class GetAccountLinkDataMiddleware : BaseMiddleware
    {
        private readonly FlowController _flowController;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;

        public GetAccountLinkDataMiddleware(RequestDelegate next, IAccountLinkHandlerAdapter linkHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            FlowController flowController,
            ILogger<GetAccountLinkDataMiddleware> logger) : base(next, coreConfiguration,
            cacheStore, localizationService, logger)
        {
            _linkHandlerAdapter = linkHandlerAdapter;
            _flowController = flowController;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var cacheItem = await GetRequestRelatedCacheItemAsync();

            if (!cacheItem.IsValidForLink)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            var culture = GetRequestCulture(httpContext);

            var profile = await _linkHandlerAdapter.GetUserProfileAsync(cacheItem.DID);

            var jwt = OwnIdProvider.GenerateProfileWithConfigDataJwt(RequestIdentity.Context,
                _flowController.GetNextStep(cacheItem, StepType.Starting), cacheItem.DID,
                profile, culture.Name);

            await Json(httpContext, new JwtContainer(jwt), StatusCodes.Status200OK);
        }
    }
}