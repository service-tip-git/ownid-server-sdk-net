using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;
using OwnIdSdk.NetCore3.Web.Middlewares;

namespace OwnIdSdk.NetCore3.Web
{
    public static class MiddlewareExtensions
    {
        public static void UseOwnId(this IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapMiddlewarePost("ownid", 
                builder => builder.UseMiddleware<GenerateContextMiddleware>());
            routeBuilder.MapMiddlewareGet("ownid/{context}/challenge",
                builder => builder.UseMiddleware<GetChallengeJwtMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/challenge",
                builder => builder.UseMiddleware<SaveProfileMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/status",
                builder => builder.UseMiddleware<GetChallengeStatusMiddleware>());

            app.UseRouter(routeBuilder.Build());
        }

        public static void AddOwnId<TChallengeHandler, TCacheStore>(this IServiceCollection services,
            ProviderConfiguration config)
            where TChallengeHandler : class, IChallengeHandler
            where TCacheStore : class, ICacheStore
        {
            // change from singleton
            services.AddSingleton<ICacheStore, TCacheStore>();
            services.AddTransient<IChallengeHandler, TChallengeHandler>();
            services.AddSingleton(config);
        }
    }
}