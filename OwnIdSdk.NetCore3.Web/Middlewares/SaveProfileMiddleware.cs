using System.Collections.Generic;
using System.Net;
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
    public class SaveProfileMiddleware : BaseMiddleware
    {
        private readonly IUserHandlerAdapter _userHandlerAdapter;
        private readonly ILogger<SaveProfileMiddleware> _logger;

        public SaveProfileMiddleware(RequestDelegate next, IUserHandlerAdapter userHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration, ICacheStore cacheStore,
            ILocalizationService localizationService, ILogger<SaveProfileMiddleware> logger) : base(next, coreConfiguration, cacheStore,
            localizationService, logger)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _logger = logger;
        }

        protected override async Task Execute(HttpContext context)
        {
            if (!TryGetRequestIdentity(context, out var requestIdentity) ||
                !OwnIdProvider.IsContextFormatValid(requestIdentity.Context))
            {
                _logger.LogError("Failed request identity validation");
                NotFound(context.Response);
                return;
            }

            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
            if (cacheItem == null || cacheItem.RequestToken != requestIdentity.RequestToken ||
                string.IsNullOrEmpty(cacheItem.ResponseToken) ||
                cacheItem.ResponseToken != requestIdentity.ResponseToken)
            {
                _logger.LogError("No such cache item or incorrect request/response token");
                NotFound(context.Response);
                return;
            }

            var request = await JsonSerializer.DeserializeAsync<JwtContainer>(context.Request.Body);

            if (string.IsNullOrEmpty(request?.Jwt))
            {
                BadRequest(context.Response);
                return;
            }

            var (jwtContext, userData) = OwnIdProvider.GetDataFromJwt<UserProfileData>(request.Jwt);
            
            if (jwtContext != requestIdentity.Context )
            {
                BadRequest(context.Response);
                return;
            }

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
                await OwnIdProvider.FinishAuthFlowSessionAsync(requestIdentity.Context, userData.DID);
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