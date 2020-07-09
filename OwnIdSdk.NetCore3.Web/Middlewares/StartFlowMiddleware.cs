using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Exceptions;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Extensions;
using OwnIdSdk.NetCore3.Web.FlowEntries.RequestHandling;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken)]
    public class StartFlowMiddleware : BaseMiddleware
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;
        private readonly FlowController _flowController;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;

        public StartFlowMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<StartFlowMiddleware> logger, FlowController flowController,
            IAccountRecoveryHandler accountRecoveryHandler = null,
            IAccountLinkHandlerAdapter linkHandlerAdapter = null) : base(next,
            coreConfiguration, cacheStore, localizationService, logger)
        {
            _flowController = flowController;
            _accountRecoveryHandler = accountRecoveryHandler;
            _linkHandlerAdapter = linkHandlerAdapter;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var cacheItem = await GetRequestRelatedCacheItemAsync();

            if (cacheItem.Status == CacheItemStatus.Finished)
                throw new RequestValidationException("Flow is already finished");

            var culture = GetRequestCulture(httpContext).Name;

            (string Jwt, string Hash) data;

            switch (cacheItem.ChallengeType)
            {
                case ChallengeType.Register:
                case ChallengeType.Login:
                    data = StartAuthorize(cacheItem, culture);
                    break;
                case ChallengeType.Link:
                    data = await StartAccountLinkingAsync(cacheItem, culture);
                    break;
                case ChallengeType.Recover:
                    data = await StartAccountRecoveryAsync(cacheItem, culture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await OwnIdProvider.SetSecurityTokensAsync(cacheItem.Context, RequestIdentity.RequestToken,
                data.Hash.GetUrlEncodeString());

            await Json(httpContext, new JwtContainer(data.Jwt), StatusCodes.Status200OK);
        }

        private (string jwt, string hash) StartAuthorize(CacheItem cacheItem, string locale)
        {
            if (!cacheItem.IsValidForLoginRegister)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Login or Register challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            var jwt = OwnIdProvider.GenerateProfileConfigJwt(RequestIdentity.Context,
                _flowController.GetNextStep(cacheItem, StepType.Starting), locale, true);

            return (jwt, OwnIdProvider.GetJwtHash(jwt));
        }

        private async Task<(string jwt, string hash)> StartAccountLinkingAsync(CacheItem cacheItem, string locale)
        {
            if (!cacheItem.IsValidForLink)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            var step = _flowController.GetNextStep(cacheItem, StepType.Starting);

            if (cacheItem.FlowType == FlowType.LinkWithPin)
            {
                var pin = await OwnIdProvider.SetSecurityCode(cacheItem.Context);
                var pinStepJwt = OwnIdProvider.GeneratePinStepJwt(cacheItem.Context, step, pin, locale);
                return (pinStepJwt, OwnIdProvider.GetJwtHash(pinStepJwt));
                ;
            }

            var profile = await _linkHandlerAdapter.GetUserProfileAsync(cacheItem.DID);
            var jwt = OwnIdProvider.GenerateProfileWithConfigDataJwt(RequestIdentity.Context,
                step, cacheItem.DID, profile, locale, true);
            return (jwt, OwnIdProvider.GetJwtHash(jwt));
        }

        private async Task<(string jwt, string hash)> StartAccountRecoveryAsync(CacheItem cacheItem, string locale)
        {
            if (!cacheItem.IsValidForRecover)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Recover challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            var step = _flowController.GetNextStep(cacheItem, StepType.Starting);

            if (cacheItem.FlowType == FlowType.RecoverWithPin)
            {
                var pin = await OwnIdProvider.SetSecurityCode(cacheItem.Context);
                var pinStepJwt = OwnIdProvider.GeneratePinStepJwt(cacheItem.Context, step, pin, locale);
                return (pinStepJwt, OwnIdProvider.GetJwtHash(pinStepJwt));
            }

            // Recover access and get user profile
            var recoverResult = await _accountRecoveryHandler.RecoverAsync(cacheItem.Payload);
            var jwt = OwnIdProvider.GenerateProfileWithConfigDataJwt(RequestIdentity.Context, step,
                recoverResult.DID, recoverResult.Profile, locale, true);
            return (jwt, OwnIdProvider.GetJwtHash(jwt));
        }
    }
}