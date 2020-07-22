using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        private readonly uint _expiration;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly IUrlProvider _urlProvider;

        public GenerateContextMiddleware(
            RequestDelegate next
            , ICacheStore cacheStore
            , IOwnIdCoreConfiguration coreConfiguration
            , ILocalizationService localizationService
            , ILogger<GenerateContextMiddleware> logger
            , IUrlProvider urlProvider
            , IAccountLinkHandlerAdapter linkHandlerAdapter = null
        ) : base(next,
            coreConfiguration,
            cacheStore, localizationService, logger)
        {
            _urlProvider = urlProvider;
            _linkHandlerAdapter = linkHandlerAdapter;
            _expiration = coreConfiguration.CacheExpirationTimeout;
        }

        protected override async Task Execute(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var request = await JsonSerializer.DeserializeAsync<GenerateContextRequest>(httpContext.Request.Body);
            httpContext.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(request.Type) ||
                !Enum.TryParse(request.Type, true, out ChallengeType challengeType) ||
                challengeType == ChallengeType.Link && _linkHandlerAdapter == null
            )
            {
                BadRequest(httpContext.Response);
                return;
            }

            var challengeContext = OwnIdProvider.GenerateContext();
            var nonce = OwnIdProvider.GenerateNonce();

            string did = null;
            string payload = null;
            FlowType flowType;

            switch (challengeType)
            {
                case ChallengeType.Register:
                case ChallengeType.Login:
                    flowType = !request.IsPartial ? FlowType.Authorize : FlowType.PartialAuthorize;
                    break;
                case ChallengeType.Link:
                    did = await _linkHandlerAdapter.GetCurrentUserIdAsync(httpContext.Request);
                    flowType = request.IsQr ? FlowType.LinkWithPin : FlowType.Link;
                    break;
                case ChallengeType.Recover:
                    payload = request.Payload.ToString();
                    flowType = request.IsQr ? FlowType.RecoverWithPin : FlowType.Recover;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await OwnIdProvider.CreateAuthFlowSessionItemAsync(challengeContext, nonce, challengeType, flowType, did,
                payload);

            await Json(
                httpContext
                , new GetChallengeLinkResponse(
                    challengeContext
                    , _urlProvider.GetWebAppWithCallbackUrl(_urlProvider.GetStartFlowUrl(challengeContext)).ToString()
                    , nonce
                    , _expiration)
                , StatusCodes.Status200OK
                , false);
        }
    }
}