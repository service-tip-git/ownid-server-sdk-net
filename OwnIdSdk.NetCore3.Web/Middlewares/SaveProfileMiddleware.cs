using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public class SaveProfileMiddleware : BaseMiddleware
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public SaveProfileMiddleware(RequestDelegate next, IUserHandlerAdapter userHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration, ICacheStore cacheStore,
            ILocalizationService localizationService) : base(next, coreConfiguration, cacheStore,
            localizationService)
        {
            _userHandlerAdapter = userHandlerAdapter;
        }

        protected override async Task Execute(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var challengeContext = routeData.Values["context"]?.ToString();
            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(challengeContext);

            if (string.IsNullOrEmpty(challengeContext) || !OwnIdProvider.IsContextFormatValid(challengeContext) ||
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

            var (jwtContext, userData) = OwnIdProvider.GetProfileDataFromJwt(request.Jwt);

            // preventing data substitution 
            challengeContext = jwtContext;

            var formContext = _userHandlerAdapter.CreateUserDefinedContext(userData, LocalizationService);

            formContext.Validate();

            if (formContext.HasErrors)
            {
                var response = new BadRequestResponse
                {
                    FieldErrors = formContext.FieldErrors as IDictionary<string, IList<string>>
                };
                await Json(context, response, (int) HttpStatusCode.BadRequest);
                return;
            }

            await _userHandlerAdapter.UpdateProfileAsync(formContext);

            if (!formContext.HasErrors)
            {
                await OwnIdProvider.SetDIDAsync(challengeContext, userData.DID);
                Ok(context.Response);
            }
            else
            {
                var response = new BadRequestResponse
                {
                    FieldErrors = formContext.FieldErrors as IDictionary<string, IList<string>>,
                    GeneralErrors = formContext.GeneralErrors
                };
                await Json(context, response, (int) HttpStatusCode.BadRequest);
            }
        }
    }
}