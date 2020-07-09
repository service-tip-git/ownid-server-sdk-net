using System;
using System.Linq;
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

namespace OwnIdSdk.NetCore3.Web.Middlewares.Approval
{
    [RequestDescriptor(BaseRequestFields.Context | BaseRequestFields.RequestToken | BaseRequestFields.ResponseToken)]
    public class GetActionApprovalStatusMiddleware : BaseMiddleware
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;
        private readonly FlowController _flowController;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;

        public GetActionApprovalStatusMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<GetActionApprovalStatusMiddleware> logger, FlowController flowController,
            IAccountRecoveryHandler accountRecoveryHandler = null,
            IAccountLinkHandlerAdapter linkHandlerAdapter = null) : base(next, coreConfiguration, cacheStore,
            localizationService, logger)
        {
            _flowController = flowController;
            _accountRecoveryHandler = accountRecoveryHandler;
            _linkHandlerAdapter = linkHandlerAdapter;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            var cacheItem = await GetRequestRelatedCacheItemAsync();
            var locale = GetRequestCulture(httpContext).Name;

            if (cacheItem.Status == CacheItemStatus.Approved)
            {
                string jwt;

                switch (cacheItem.ChallengeType)
                {
                    case ChallengeType.Link:
                        jwt = await StartAccountLinkingAsync(cacheItem, locale);
                        break;
                    case ChallengeType.Recover:
                        jwt = await StartAccountRecoveryAsync(cacheItem, locale);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                await Json(httpContext, new
                {
                    status = "approved", jwt
                }, StatusCodes.Status200OK);
            }
            else if (cacheItem.Status == CacheItemStatus.Declined)
            {
                var jwt = OwnIdProvider.GenerateFinalStepJwt(cacheItem.Context,
                    _flowController.GetNextStep(cacheItem, StepType.ApprovePin), locale
                );

                await Json(httpContext, new
                {
                    status = "declined", jwt
                }, StatusCodes.Status200OK);
            }
            else
            {
                var status = cacheItem.Status.ToString();
                await Json(httpContext, new
                {
                    status = status.First().ToString().ToLower() + status.Substring(1)
                }, StatusCodes.Status200OK);
            }
        }

        private async Task<string> StartAccountLinkingAsync(CacheItem cacheItem, string locale)
        {
            if (!cacheItem.IsValidForLink)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            var profile = await _linkHandlerAdapter.GetUserProfileAsync(cacheItem.DID);
            var tokenData = OwnIdProvider.GenerateProfileWithConfigDataJwt(RequestIdentity.Context,
                _flowController.GetNextStep(cacheItem, StepType.ApprovePin),
                cacheItem.DID,
                profile, locale);
            return tokenData;
        }

        private async Task<string> StartAccountRecoveryAsync(CacheItem cacheItem, string locale)
        {
            if (!cacheItem.IsValidForRecover)
                throw new RequestValidationException(
                    "Cache item should be not Finished with Recover challenge type. " +
                    $"Actual Status={cacheItem.Status.ToString()} ChallengeType={cacheItem.ChallengeType}");

            // Recover access and get user profile
            var recoverResult = await _accountRecoveryHandler.RecoverAsync(cacheItem.Payload);
            var data = OwnIdProvider.GenerateProfileWithConfigDataJwt(RequestIdentity.Context,
                _flowController.GetNextStep(cacheItem, StepType.ApprovePin),
                recoverResult.DID,
                recoverResult.Profile, locale);
            return data;
        }
    }
}