using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web
{
    public class OwnIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Provider _provider;

        public OwnIdMiddleware(RequestDelegate next, IChallengeHandler handler, ICacheStore cacheStore, ProviderConfiguration providerConfiguration)
        {
            _next = next;
            _provider = new Provider(cacheStore, providerConfiguration);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: research for better routing

            if (context.Request.Method == HttpMethods.Post && string.IsNullOrEmpty(context.Request.Path))
            {
                var sessionContext = _provider.GenerateContext();

                await context.Response.WriteAsync(JsonSerializer.Serialize(new {url = _provider.GetDeepLink(sessionContext), context = sessionContext}));
                return;
            }
            
            await _next(context);
        }
        
        
    }
}