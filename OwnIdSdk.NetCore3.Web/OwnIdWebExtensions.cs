using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Web.Configuration;
using OwnIdSdk.NetCore3.Web.Features;
using OwnIdSdk.NetCore3.Web.Middlewares;
using OwnIdSdk.NetCore3.Web.Middlewares.Approval;
using OwnIdSdk.NetCore3.Web.Middlewares.Authorize;
using OwnIdSdk.NetCore3.Web.Middlewares.Link;
using OwnIdSdk.NetCore3.Web.Middlewares.Recover;

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
            routeBuilder.MapMiddlewarePost("ownid/{context}/start",
                builder => builder.UseMiddleware<StartFlowMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/challenge",
                builder => builder.UseMiddleware<SaveProfileMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/challenge/partial",
                builder => builder.UseMiddleware<SavePartialProfileMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/status",
                builder => builder.UseMiddleware<GetChallengeStatusMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/approve",
                builder => builder.UseMiddleware<ApproveActionMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/approval-status",
                builder => builder.UseMiddleware<GetActionApprovalStatusMiddleware>());

            var configuration = app.ApplicationServices.GetService<OwnIdConfiguration>();

            if (configuration.HasFeature<AccountLinkFeature>())
                routeBuilder.MapMiddlewarePost("ownid/{context}/link",
                    builder => builder.UseMiddleware<SaveAccountLinkMiddleware>());

            if (configuration.HasFeature<AccountRecoveryFeature>())
                routeBuilder.MapMiddlewarePost("ownid/{context}/recover",
                    builder => builder.UseMiddleware<SaveAccountPublicKeyMiddleware>());

            app.UseRouter(routeBuilder.Build());
        }

        /// <summary>
        ///     Adds OwnId features and services with configuration
        /// </summary>
        /// <remarks>Extension method for <see cref="IServiceCollection" /></remarks>
        /// <param name="services"></param>
        /// <param name="configureAction">Configuration builder. Allows to tune all available settings and features</param>
        public static void AddOwnId(this IServiceCollection services,
            Action<OwnIdConfigurationBuilder> configureAction = null)
        {
            var builder = new OwnIdConfigurationBuilder(services);
            builder.UseInMemoryCacheStore();
            configureAction?.Invoke(builder);
            builder.Configuration.Validate();
            builder.Configuration.IntegrateFeatures(services);
        }
    }
}