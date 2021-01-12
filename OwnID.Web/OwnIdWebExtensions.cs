using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OwnID.Web.Configuration;
using OwnID.Web.Features;
using OwnID.Web.Middlewares;
using OwnID.Web.Middlewares.Approval;
using OwnID.Web.Middlewares.Authorize;
using OwnID.Web.Middlewares.Fido2;
using OwnID.Web.Middlewares.Link;
using OwnID.Web.Middlewares.MagicLink;
using OwnID.Web.Middlewares.Recover;

namespace OwnID.Web
{
    public static class OwnIdWebExtensions
    {
        /// <summary>
        ///     Adds required for the OwnId authorization process Middlewares
        /// </summary>
        public static void UseOwnId(this IApplicationBuilder app)
        {
            var configuration = app.ApplicationServices.GetService<OwnIdConfiguration>();

            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapMiddlewarePost("ownid",
                builder => builder.UseMiddleware<GenerateContextMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/start",
                builder => builder.UseMiddleware<StartFlowMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/stop",
                builder => builder.UseMiddleware<StopFlowMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/start/accept",
                builder => builder.UseMiddleware<AcceptStartFlowMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/users/existence",
                builder => builder.UseMiddleware<CheckUserExistenceMiddleware>());
            routeBuilder.MapMiddlewarePost($"ownid/{{context}}/auth-type/fido2",
                builder => builder.UseMiddleware<UpgradeToFIDO2Middleware>());
            routeBuilder.MapMiddlewarePost($"ownid/{{context}}/auth-type/passcode",
                builder => builder.UseMiddleware<UpgradeToPasscodeMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/conn-recovery",
                builder => builder.UseMiddleware<InternalConnectionRecoveryMiddleware>());
            // routeBuilder.MapMiddlewarePost("ownid/{context}/challenge",
            //     builder => builder.UseMiddleware<SaveProfileMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/challenge/partial",
                builder => builder.UseMiddleware<SavePartialProfileMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/challenge/fido2",
                builder => builder.UseMiddleware<Fido2AuthMiddleware>());
            routeBuilder.MapMiddlewareGet("ownid/{context}/fido2/settings",
                builder => builder.UseMiddleware<Fido2SettingsMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/approve",
                builder => builder.UseMiddleware<ApproveActionMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/{context}/approval-status",
                builder => builder.UseMiddleware<GetActionApprovalStatusMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/status",
                builder => builder.UseMiddleware<GetChallengeStatusMiddleware>());
            routeBuilder.MapMiddlewarePost("ownid/connections",
                builder => builder.UseMiddleware<AddConnectionMiddleware>());

            if (configuration.HasFeature<AccountLinkFeature>())
                routeBuilder.MapMiddlewarePost("ownid/{context}/link",
                    builder => builder.UseMiddleware<SaveAccountLinkMiddleware>());

            if (configuration.HasFeature<AccountRecoveryFeature>())
                routeBuilder.MapMiddlewarePost("ownid/{context}/recover",
                    builder => builder.UseMiddleware<SaveAccountPublicKeyMiddleware>());

            if (configuration.HasFeature<MagicLinkFeature>())
            {
                routeBuilder.MapMiddlewareGet("ownid/magic",
                    builder => builder.UseMiddleware<SendMagicLinkMiddleware>());
                routeBuilder.MapMiddlewarePost("ownid/magic",
                    builder => builder.UseMiddleware<ExchangeMagicLinkMiddleware>());
            }

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