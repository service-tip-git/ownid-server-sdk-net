using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web
{
    public static class MiddlewareExtensions
    {
        public static void UseOwnId(this IApplicationBuilder app)
        {
            app.Map("/ownid",
                builder => { builder.UseMiddleware<OwnIdMiddleware>(); });
        }

        public static void AddOwnId<TChallengeHandler, TCacheStore>(this IServiceCollection services, ProviderConfiguration config)
            where TChallengeHandler : class, IChallengeHandler 
            where TCacheStore : class, ICacheStore
        {
            // change from singleton
            services.AddSingleton<ICacheStore, TCacheStore>();
            services.AddScoped<IChallengeHandler, TChallengeHandler>();
            services.AddSingleton(config);
        }
    }
}