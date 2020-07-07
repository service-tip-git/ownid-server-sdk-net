using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly uint _expiration;

        public GenerateContextMiddleware(
            RequestDelegate next
            , ICacheStore cacheStore
            , IOwnIdCoreConfiguration coreConfiguration
            , ILocalizationService localizationService
            , ILogger<GenerateContextMiddleware> logger
            , IAccountLinkHandlerAdapter linkHandlerAdapter = null
        ) : base(next,
            coreConfiguration,
            cacheStore, localizationService, logger)
        {
            _linkHandlerAdapter = linkHandlerAdapter;
            _expiration = coreConfiguration.CacheExpirationTimeout;
        }

        protected override async Task Execute(HttpContext context)
        {
            context.Request.EnableBuffering();
            var request = await JsonSerializer.DeserializeAsync<GenerateContextRequest>(context.Request.Body);
            context.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(request.Type) ||
                !Enum.TryParse(request.Type, true, out ChallengeType challengeType) ||
                challengeType == ChallengeType.Link && _linkHandlerAdapter == null
            )
            {
                BadRequest(context.Response);
                return;
            }

            var challengeContext = OwnIdProvider.GenerateContext();
            var nonce = OwnIdProvider.GenerateNonce();

            string did = null;
            string payload = null;
            switch (challengeType)
            {
                case ChallengeType.Register:
                    break;
                case ChallengeType.Login:
                    break;
                case ChallengeType.Link:
                    did = await _linkHandlerAdapter.GetCurrentUserIdAsync(context.Request);
                    break;
                case ChallengeType.Recover:
                    payload = request.Payload.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await OwnIdProvider.CreateAuthFlowSessionItemAsync(challengeContext, nonce, challengeType, did, payload);

            await Json(
                context
                , new GetChallengeLinkResponse(
                    challengeContext
                    , OwnIdProvider.GetDeepLink(challengeContext, challengeType)
                    , nonce
                    , _expiration)
                , StatusCodes.Status200OK
                , false);
        }
    }
}