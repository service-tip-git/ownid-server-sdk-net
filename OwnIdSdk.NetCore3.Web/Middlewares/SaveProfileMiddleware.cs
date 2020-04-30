using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
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

            // add check for context
            if (string.IsNullOrEmpty(challengeContext) || !_provider.IsContextValid(challengeContext))
            {
                NotFound(context.Response);
                return;
            }
            
            var request = await JsonSerializer.DeserializeAsync<JwtContainer>(context.Request.Body);

            if (string.IsNullOrEmpty(request?.Jwt))
            {
                BadRequest(context.Response);
                return;
            }
            
            var (jwtContext, userData) =  _provider.GetProfileDataFromJwt(request.Jwt);

            // preventing data substitution 
            challengeContext = jwtContext;
            await _challengeHandler.UpdateProfileAsync(userData.DID, userData.Profile, userData.PublicKey);
            await _provider.SetDIDAsync(challengeContext, userData.DID);

            Ok(context.Response);
        }
    }
}