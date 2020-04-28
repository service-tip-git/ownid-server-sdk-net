using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Middlewares
{
    public abstract class BaseMiddleware
    {
        protected readonly RequestDelegate _next;
        protected readonly IChallengeHandler _challengeHandler;
        protected readonly Provider _provider;

        protected BaseMiddleware(RequestDelegate next, IChallengeHandler challengeHandler, ICacheStore cacheStore,
            ProviderConfiguration providerConfiguration)
        {
            _next = next;
            _challengeHandler = challengeHandler;
            _provider = new Provider(cacheStore, providerConfiguration);
            
        }

        public abstract Task InvokeAsync(HttpContext context);
        
        protected async Task Ok<T>(HttpResponse response, T responseBody) where T : class
        {
            Ok(response);
            response.ContentType = "application/json";
            await response.WriteAsync(JsonSerializer.Serialize(responseBody));
        }

        protected void Ok(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.OK;
        }

        protected void NotFound(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.NotFound;
        }
        
        protected void BadRequest(HttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.BadRequest;
        }
    }
}