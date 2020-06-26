using System;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public abstract class BaseMiddleware
    {
        protected readonly ILogger Logger;
        protected readonly ILocalizationService LocalizationService;
        protected readonly RequestDelegate Next;
        protected readonly OwnIdProvider OwnIdProvider;

        protected BaseMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService, ILogger logger)
        {
            Next = next;
            LocalizationService = localizationService;
            OwnIdProvider = new OwnIdProvider(coreConfiguration, cacheStore, LocalizationService);
            Logger = logger;
        }

        public string Context { get; private set; }

        public async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            Context = routeData.Values["context"]?.ToString();

            using var scope = Logger.BeginScope(new
            {
                context = Context
            });
            
            await InterceptErrors(Execute, context);
        }

        protected abstract Task Execute(HttpContext context);

        protected void Ok(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.OK;
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

        protected CultureInfo GetRequestCulture(HttpContext context)
        {
            var rqf = context.Features.Get<IRequestCultureFeature>();
            return rqf.RequestCulture.Culture;
        }

        private async Task InterceptErrors(Func<HttpContext, Task> functionToInvoke, HttpContext context)
        {
            try
            {
                await functionToInvoke(context);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Middleware error interceptor");
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("Operation failed. Please try later");
            }
        }

        protected bool TryGetRequestIdentity(HttpContext context,
            out (string Context, string RequestToken, string ResponseToken) identity)
        {
            var isValidRequestIdentity = !string.IsNullOrWhiteSpace(Context) &&
                                         context.Request.Query.TryGetValue("rt", out var requestToken) &&
                                         !string.IsNullOrWhiteSpace(requestToken.ToString());

            context.Request.Query.TryGetValue("rst", out var responseToken);

            identity = isValidRequestIdentity ? (Context, requestToken.ToString(), responseToken) : default;

            return isValidRequestIdentity;
        }
    }
}