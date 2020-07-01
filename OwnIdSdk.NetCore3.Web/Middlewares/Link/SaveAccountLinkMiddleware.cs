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

namespace OwnIdSdk.NetCore3.Web.Middlewares.Link
{
    public class SaveAccountLinkMiddleware : BaseMiddleware
    {
        private readonly IAccountLinkHandlerAdapter _accountLinkHandlerAdapter;

        public SaveAccountLinkMiddleware(RequestDelegate next, IAccountLinkHandlerAdapter accountLinkHandlerAdapter,
            IOwnIdCoreConfiguration coreConfiguration, ICacheStore cacheStore, ILocalizationService localizationService,
            ILogger<SaveAccountLinkMiddleware> logger)
            : base(next, coreConfiguration, cacheStore, localizationService, logger)
        {
            _accountLinkHandlerAdapter = accountLinkHandlerAdapter;
        }

        protected override async Task Execute(HttpContext context)
        {
            if (!TryGetRequestIdentity(context, out var requestIdentity) ||
                !OwnIdProvider.IsContextFormatValid(requestIdentity.Context))
            {
                Logger.LogDebug("Failed request identity validation");
                NotFound(context.Response);
                return;
            }

            var cacheItem = await OwnIdProvider.GetCacheItemByContextAsync(requestIdentity.Context);
            if (cacheItem == null || !cacheItem.IsValidForLoginRegister ||
                cacheItem.RequestToken != requestIdentity.RequestToken ||
                string.IsNullOrEmpty(cacheItem.ResponseToken) ||
                cacheItem.ResponseToken != requestIdentity.ResponseToken)
            {
                Logger.LogError("No such cache item or incorrect request/response token");
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

            if (jwtContext != requestIdentity.Context)
            {
                BadRequest(context.Response);
                return;
            }

            //preventing data substitution
            userData.DID = cacheItem.DID;

            var formContext = _accountLinkHandlerAdapter.CreateUserDefinedContext(userData, LocalizationService);
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

            await _accountLinkHandlerAdapter.OnLink(formContext);

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