using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Contracts;
using OwnIdSdk.NetCore3.Contracts.Jwt;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Exceptions;
using OwnIdSdk.NetCore3.Web.Extensions;
using OwnIdSdk.NetCore3.Web.FlowEntries.RequestHandling;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public abstract class BaseMiddleware
    {
        private readonly BaseRequestFields _fields;

        protected readonly ILocalizationService LocalizationService;
        protected readonly ILogger Logger;
        protected readonly RequestDelegate Next;
        protected readonly OwnIdProvider OwnIdProvider;

        protected BaseMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService, ILogger logger)
        {
            Next = next;
            LocalizationService = localizationService;
            OwnIdProvider = new OwnIdProvider(coreConfiguration, cacheStore, LocalizationService);
            Logger = logger;

            var attrs = GetType().GetCustomAttribute(typeof(RequestDescriptorAttribute));

            if (attrs != null && attrs is RequestDescriptorAttribute descriptor)
                _fields = descriptor.Fields;
            else
                _fields = BaseRequestFields.None;
        }

        protected RequestIdentity RequestIdentity { get; private set; }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            RequestIdentity = new RequestIdentity();
            var routeData = httpContext.GetRouteData();

            RequestIdentity.Context = routeData.Values["context"]?.ToString();

            using var scope = Logger.BeginScope("context: {context}", RequestIdentity.Context);

            await InterceptErrors(InternalExecuteAsync, httpContext);
        }

        protected abstract Task Execute(HttpContext httpContext);

        protected async Task<CacheItem> GetRequestRelatedCacheItemAsync()
        {
            var item = await OwnIdProvider.GetCacheItemByContextAsync(RequestIdentity.Context);

            if (item == null)
                throw new RequestValidationException($"No cache item was found with context={RequestIdentity.Context}");

            return item;
        }

        protected void ValidateCacheItemTokens(CacheItem item)
        {
            if (item.RequestToken != RequestIdentity.RequestToken)
                throw new RequestValidationException(
                    $"{nameof(RequestIdentity.RequestToken)} doesn't match. Expected={item.RequestToken} Actual={RequestIdentity.RequestToken}");

            if (item.ResponseToken != RequestIdentity.ResponseToken)
                throw new RequestValidationException(
                    $"{nameof(RequestIdentity.ResponseToken)} doesn't match. Expected={item.ResponseToken} Actual={RequestIdentity.ResponseToken}");
        }

        protected async Task<TData> GetRequestJwtDataAsync<TData>(HttpContext httpContext) where TData : ISignedData
        {
            var request = await JsonSerializer.DeserializeAsync<JwtContainer>(httpContext.Request.Body);

            if (string.IsNullOrEmpty(request?.Jwt))
                throw new RequestValidationException("No JWT was found in request");

            var (jwtContext, userData) = OwnIdProvider.GetDataFromJwt<TData>(request.Jwt);

            if (jwtContext != RequestIdentity.Context)
                throw new RequestValidationException(
                    $"Request Context({RequestIdentity.Context}) is not equal to JWT Context({jwtContext})");

            return userData;
        }

        protected CultureInfo GetRequestCulture(HttpContext context)
        {
            var rqf = context.Features.Get<IRequestCultureFeature>();
            return rqf.RequestCulture.Culture;
        }

        private async Task InternalExecuteAsync(HttpContext httpContext)
        {
            if (_fields != BaseRequestFields.None)
            {
                if (_fields.HasFlag(BaseRequestFields.Context) &&
                    (string.IsNullOrEmpty(RequestIdentity.Context) ||
                     !OwnIdProvider.IsContextFormatValid(RequestIdentity.Context)))
                    throw new RequestValidationException(
                        $"{nameof(RequestIdentity.Context)} is required and should have correct format. Path={httpContext.Request.Path.ToString()}");

                if (_fields.HasFlag(BaseRequestFields.RequestToken))
                {
                    if (httpContext.Request.Query.TryGetValue("rt", out var requestToken))
                        RequestIdentity.RequestToken = requestToken;
                    else
                        throw new RequestValidationException(
                            $"{nameof(RequestIdentity.RequestToken)} is required. Path={httpContext.Request.Path.ToString()} Query={httpContext.Request.QueryString.ToString()}");
                }

                if (_fields.HasFlag(BaseRequestFields.ResponseToken))
                {
                    if (httpContext.Request.Query.TryGetValue("rst", out var responseToken))
                        RequestIdentity.ResponseToken = responseToken.ToString().GetUrlEncodeString();
                    else
                        throw new RequestValidationException(
                            $"{nameof(RequestIdentity.ResponseToken)} is required. Path={httpContext.Request.Path.ToString()} Query={httpContext.Request.QueryString.ToString()}");
                }
            }

            await Execute(httpContext);
        }

        private async Task InterceptErrors(Func<HttpContext, Task> functionToInvoke, HttpContext httpContext)
        {
            try
            {
                await functionToInvoke(httpContext);
            }
            catch (RequestValidationException e)
            {
                Logger.LogError(e, e.Message);
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (BusinessValidationException e)
            {
                var response = new BadRequestResponse
                {
                    FieldErrors = e.FormContext.FieldErrors as IDictionary<string, IList<string>>,
                    GeneralErrors = e.FormContext.GeneralErrors
                };
                httpContext.Response.Clear();
                await Json(httpContext, response, StatusCodes.Status400BadRequest);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Middleware error interceptor");
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.ContentType = "text/html";
                await httpContext.Response.WriteAsync("Operation failed. Please try later");
            }
        }

        #region Response Shortcuts

        protected void OkNoContent(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.NoContent;
        }

        protected async Task Json<T>(HttpContext context, T responseBody, int statusCode, bool addLocaleHeader = true)
            where T : class
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            if (addLocaleHeader)
            {
                var culture = GetRequestCulture(context);
                context.Response.Headers.Add("Content-Language", culture.ToString());
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(responseBody));
        }

        protected void NotFound(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.NotFound;
        }

        protected void BadRequest(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.BadRequest;
        }

        #endregion
    }
}