using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class SaveProfileMiddleware : BaseMiddleware
    {
        public SaveProfileMiddleware(RequestDelegate next, IChallengeHandler challengeHandler, ICacheStore cacheStore,
            ProviderConfiguration providerConfiguration) : base(next, challengeHandler, cacheStore,
            providerConfiguration)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            if (string.IsNullOrEmpty(challengeContext) || !_provider.IsContextValid(challengeContext))
            {
                NotFound(context.Response);
                return;
            }

            // TODO:  _provider.GetProfileDataFromJwt();

            var profile = new { };
            var did = "ownid:did:" + Guid.NewGuid();

            await _challengeHandler.UpdateProfileAsync(did, profile);
            await _provider.SetDIDAsync(challengeContext, did);

            Ok(context.Response);
        }
    }
}