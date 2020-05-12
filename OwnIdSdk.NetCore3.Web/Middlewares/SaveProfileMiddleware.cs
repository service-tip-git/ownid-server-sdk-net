using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;
using OwnIdSdk.NetCore3.Web.FlowEntries;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class SaveProfileMiddleware : BaseMiddleware
    {
        private readonly OwnIdConfiguration _providerConfiguration;

        public SaveProfileMiddleware(RequestDelegate next, IChallengeHandler challengeHandler, ICacheStore cacheStore,
            IOptions<OwnIdConfiguration> providerConfiguration) : base(next, challengeHandler, cacheStore,
            providerConfiguration)
        {
            _providerConfiguration = providerConfiguration.Value;
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();

            // add check for context
            if (string.IsNullOrEmpty(challengeContext) || !Provider.IsContextFormatValid(challengeContext))
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
            var formContext = new UserProfileFormContext(userData, _providerConfiguration.ProfileFields);
            await ChallengeHandler.UpdateProfileAsync(formContext);

            if (!formContext.HasErrors)
            {
                await Provider.SetDIDAsync(challengeContext, userData.DID);
                Ok(context.Response);
            }
            else
            {
                var response = new BadRequestResponse
                {
                    FieldErrors = formContext.Values.ToDictionary(x => x.Key,
                        x => (IEnumerable<string>) x.Errors),
                    GeneralErrors = formContext.GeneralErrors
                };
                await Json(context.Response, response, (int) HttpStatusCode.BadRequest);
            }
        }
    }
}