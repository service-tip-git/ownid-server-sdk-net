using System;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public abstract class BaseMiddleware
    {
        protected readonly ILocalizationService LocalizationService;
        protected readonly RequestDelegate Next;
        protected readonly OwnIdProvider OwnIdProvider;

        protected BaseMiddleware(RequestDelegate next, IOwnIdCoreConfiguration coreConfiguration,
            ICacheStore cacheStore, ILocalizationService localizationService)
        {
            Next = next;
            LocalizationService = localizationService;
            OwnIdProvider = new OwnIdProvider(coreConfiguration, cacheStore, LocalizationService);
        }

        public async Task InvokeAsync(HttpContext context)
        {
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

        protected async Task InterceptErrors(Func<HttpContext, Task> functionToInvoke, HttpContext context)
        {
            try
            {
                await functionToInvoke(context);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("Operation failed. Please try later");
            }
        }
    }
}