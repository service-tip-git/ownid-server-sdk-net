using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class GenerateContextMiddleware : BaseMiddleware
    {
        public GenerateContextMiddleware(RequestDelegate next, IChallengeHandler challengeHandler,
            ICacheStore cacheStore, ProviderConfiguration providerConfiguration) : base(next, challengeHandler,
            cacheStore, providerConfiguration)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var request = await JsonSerializer.DeserializeAsync<GenerateContextRequest>(context.Request.Body);

            if (string.IsNullOrWhiteSpace(request.Type) || !Enum.TryParse(request.Type, true, out ChallengeType challengeType))
            {
                BadRequest(context.Response);
                return;
            }
            
            var challengeContext = _provider.GenerateContext();
            var nonce = _provider.GenerateNonce();

            await _provider.StoreNonceAsync(challengeContext, nonce);

            await Ok(context.Response,
                new GetChallengeLinkResponse(challengeContext, _provider.GetDeepLink(challengeContext, challengeType), nonce
                ));
        }
    }
}