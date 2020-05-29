using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        public GenerateContextMiddleware(RequestDelegate next, ICacheStore cacheStore,
            IOwnIdCoreConfiguration coreConfiguration, ILocalizationService localizationService) : base(next,
            coreConfiguration,
            cacheStore, localizationService)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var request = await JsonSerializer.DeserializeAsync<GenerateContextRequest>(context.Request.Body);

            if (string.IsNullOrWhiteSpace(request.Type) ||
                !Enum.TryParse(request.Type, true, out ChallengeType challengeType))
            {
                BadRequest(context.Response);
                return;
            }
            
            var challengeContext = OwnIdProvider.GenerateContext();
            var nonce = OwnIdProvider.GenerateNonce();

            await OwnIdProvider.StoreNonceAsync(challengeContext, nonce);
            await Json(context, new GetChallengeLinkResponse(challengeContext,
                OwnIdProvider.GetDeepLink(challengeContext, challengeType),
                nonce
            ), StatusCodes.Status200OK, false);
        }
    }
}