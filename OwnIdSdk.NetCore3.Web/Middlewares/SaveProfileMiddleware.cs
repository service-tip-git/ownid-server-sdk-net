using System.Collections.Generic;
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

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class SaveProfileMiddleware : BaseMiddleware
    {
        private readonly IChallengeHandlerAdapter _challengeHandlerAdapter;

        public SaveProfileMiddleware(RequestDelegate next, IChallengeHandlerAdapter challengeHandlerAdapter,
            ICacheStore cacheStore,
            IOptions<OwnIdConfiguration> providerConfiguration) : base(next, cacheStore,
            providerConfiguration)
        {
            _challengeHandlerAdapter = challengeHandlerAdapter;
        }

        public override async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();
            var cacheItem = await Provider.GetCacheItemByContextAsync(challengeContext);

            // add check for context
            if (string.IsNullOrEmpty(challengeContext) || !Provider.IsContextFormatValid(challengeContext) ||
                cacheItem == null)
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

            var formContext = _challengeHandlerAdapter.CreateUserDefinedContext(userData);

            formContext.Validate();

            if (formContext.HasErrors)
            {
                var response = new BadRequestResponse
                {
                    FieldErrors = formContext.FieldErrors as IDictionary<string, IList<string>>
                };
                await Json(context.Response, response, (int) HttpStatusCode.BadRequest);
                return;
            }

            await _challengeHandlerAdapter.UpdateProfileAsync(formContext);

            if (!formContext.HasErrors)
            {
                await Provider.SetDIDAsync(challengeContext, userData.DID);
                Ok(context.Response);
            }
            else
            {
                var response = new BadRequestResponse
                {
                    FieldErrors = formContext.FieldErrors as IDictionary<string, IList<string>>,
                    GeneralErrors = formContext.GeneralErrors
                };
                await Json(context.Response, response, (int) HttpStatusCode.BadRequest);
            }
        }
    }
}