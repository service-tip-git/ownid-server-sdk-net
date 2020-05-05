using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
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
            if (string.IsNullOrEmpty(challengeContext) || !Provider.IsContextValid(challengeContext))
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

            var (jwtContext, userData) = Provider.GetProfileDataFromJwt(request.Jwt);

            // preventing data substitution 
            challengeContext = jwtContext;
            await ChallengeHandler.UpdateProfileAsync(userData.DID, userData.Profile, userData.PublicKey);
            await Provider.SetDIDAsync(challengeContext, userData.DID);

            Ok(context.Response);
        }
    }
}