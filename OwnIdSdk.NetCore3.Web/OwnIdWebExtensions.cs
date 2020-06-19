using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Web.Configuration;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Features;
using OwnIdSdk.NetCore3.Web.FlowEntries;
using OwnIdSdk.NetCore3.Web.Middlewares;

namespace OwnIdSdk.NetCore3.Web
{
    public static class OwnIdWebExtensions
    {
        /// <summary>
        ///     Adds required for the OwnId authorization process Middlewares
        /// </summary>
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

            var configuration = app.ApplicationServices.GetService<OwnIdConfiguration>();

            if (configuration.FindFeature<AccountLinkFeature>() != null)
            {
                routeBuilder.MapMiddlewarePost("ownid/{context}/link",
                    builder => builder.UseMiddleware<SaveAccountLinkMiddleware>());
                routeBuilder.MapMiddlewareGet("ownid/{context}/link",
                    builder => builder.UseMiddleware<GetAccountLinkDataMiddleware>());
            }

            app.UseRouter(routeBuilder.Build());
        }

        /// <summary>
        ///     Adds OwnId features and services with configuration
        /// </summary>
        /// <remarks>Extension method for <see cref="IServiceCollection" /></remarks>
        /// <param name="configureAction">Configuration builder. Allows to tune all available settings and features</param>
        public static void AddOwnId(this IServiceCollection services,
            Action<OwnIdConfigurationBuilder> configureAction = null)
        {
            var builder = new OwnIdConfigurationBuilder(services);
            builder.UseInMemoryCacheStore();
            configureAction?.Invoke(builder);
            builder.Configuration.Validate();
            builder.Configuration.IntegrateFeatures(services);
            services.TryAddTransient<IAccountLinkHandlerAdapter, AccountLinkHandlerAdapter<LocalizationFeature>>();
        }
    }
}