using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Abstractions;
using OwnIdSdk.NetCore3.Web.FlowEntries;
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

        public static void AddOwnId<TModel, TChallengeHandler, TCacheStore>(this IServiceCollection services,
            Action<OwnIdConfiguration> setupAction)
            where TChallengeHandler : class, IChallengeHandler<TModel>
            where TCacheStore : class, ICacheStore
            where TModel : class
        {
            services.AddOptions<OwnIdConfiguration>()
                .Configure(setupAction)
                .PostConfigure(x =>
                {
                    x.SetProfileModel<TModel>();
                    x.ProfileConfiguration.BuildMetadata();
                });

            services.AddSingleton<IValidateOptions<OwnIdConfiguration>, OwnIdConfigurationValidator>();

            //Validate configuration and add to DI container
            services.AddSingleton(container =>
            {
                try
                {
                    return container.GetService<IOptions<OwnIdConfiguration>>().Value;
                }
                catch (OptionsValidationException ex)
                {
                    foreach (var validationFailure in ex.Failures)
                        Console.Error.WriteLine(
                            $"appSettings section '{nameof(OwnIdConfiguration)}' failed validation. Reason: {validationFailure}");

                    throw;
                }
            });

            // change from singleton
            services.AddSingleton<ICacheStore, TCacheStore>();
            services.AddTransient<IChallengeHandler<TModel>, TChallengeHandler>();
            services.AddTransient<IChallengeHandlerAdapter, ChallengeHandlerAdapter<TModel>>();
        }
    }
}