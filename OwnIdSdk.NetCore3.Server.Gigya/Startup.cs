using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Redis;
using OwnIdSdk.NetCore3.Server.Gigya.External;
using OwnIdSdk.NetCore3.Server.Gigya.Middlewares.SecurityHeaders;
using OwnIdSdk.NetCore3.Web;
using OwnIdSdk.NetCore3.Web.Gigya;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class Startup
    {
        private const string CorsPolicyName = "AllowAll";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigureLogging();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var ownIdSection = Configuration.GetSection("ownid");
            var gigyaSection = Configuration.GetSection("gigya");
            var isDevelopment = Configuration.GetValue("OwnIdDevelopmentMode", false);
            var topDomain = ownIdSection["top_domain"];
            var webAppUrl = new Uri(ownIdSection["web_app_url"] ?? Constants.OwinIdApplicationAddress);

            services.AddCors(x =>
            {
                x.AddPolicy(CorsPolicyName, builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowCredentials();
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains();

                    var originsList = new List<string>();

                    if (isDevelopment)
                    {
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
                    }
                    else
                    {
                        originsList.Add($"https://*.{topDomain}");
                        originsList.Add($"https://{topDomain}");
                        originsList.Add(webAppUrl.ToString().TrimEnd('/'));
                    }

                    builder.WithOrigins(originsList.ToArray());
                });
            });

            services.AddOwnId(
                builder =>
                {
                    var loginTypeString = gigyaSection["login_type"];

                    if (string.IsNullOrEmpty(loginTypeString) || !Enum.TryParse(gigyaSection["login_type"], true,
                        out GigyaLoginType loginType)) loginType = GigyaLoginType.Session;

                    builder.UseGigya(gigyaSection["data_center"], gigyaSection["api_key"], gigyaSection["secret"],
                        gigyaSection["user_key"], loginType);
                    builder.SetKeys(ownIdSection["pub_key"], ownIdSection["private_key"]);

                    switch (ownIdSection["cache_type"])
                    {
                        case "web-cache":
                            builder.UseWebCacheStore();
                            break;
                        case "redis":
                            builder.UseCacheStore<RedisCacheStore>(ServiceLifetime.Singleton);
                            break;
                    }

                    builder.WithBaseSettings(x =>
                    {
                        x.DID = ownIdSection["did"];
                        x.Name = ownIdSection["name"];
                        x.Description = ownIdSection["description"];
                        x.Icon = ownIdSection["icon"];
                        x.CallbackUrl = new Uri(ownIdSection["callback_url"]);
                        x.TopDomain = topDomain;

                        x.CacheExpirationTimeout = ownIdSection.GetValue("cache_expiration",
                            (uint) TimeSpan.FromMinutes(10).TotalMilliseconds);

                        if (ownIdSection.Key.Contains("maximum_number_of_connected_devices"))
                            x.MaximumNumberOfConnectedDevices =
                                ownIdSection.GetValue<uint>("maximum_number_of_connected_devices");

                        x.OwnIdApplicationUrl = webAppUrl;
                        x.OverwriteFields = ownIdSection.GetValue<bool>("overwrite_fields");

                        x.AuthenticationMode =
                            ownIdSection.GetValue("authentication_mode", AuthenticationModeType.OwnIdOnly);

                        if (x.AuthenticationMode.IsFido2Enabled())
                        {
                            if (!string.IsNullOrWhiteSpace(ownIdSection["fido2_passwordless_page_url"]))
                                x.Fido2.PasswordlessPageUrl = new Uri(ownIdSection["fido2_passwordless_page_url"]);

                            x.Fido2.RelyingPartyId = ownIdSection["fido2_relying_party_id"];
                            x.Fido2.RelyingPartyName = ownIdSection["fido2_relying_party_name"];
                            x.Fido2.UserName = ownIdSection["fido2_user_name"];
                            x.Fido2.UserDisplayName = ownIdSection["fido2_user_display_name"];

                            if(!string.IsNullOrWhiteSpace(ownIdSection["fido2_origin"]))
                                x.Fido2.Origin = new Uri(ownIdSection["fido2_origin"]);
                        }

                        //for development cases
                        x.IsDevEnvironment = isDevelopment;
                    });
                });

            // TODO: not for prod
            services.AddHostedService<CpuMemoryLogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(x =>
            {
                x.AddSupportedCultures("ru", "es", "en");
                x.DefaultRequestCulture = new RequestCulture("en", "en");
                x.AddSupportedUICultures("ru", "en", "es");
            });
            app.UseCors(CorsPolicyName);

            // TODO: not for prod
            app.UseMiddleware<LogRequestMiddleware>();
            var routeBuilder = new RouteBuilder(app);
            routeBuilder.MapMiddlewarePost("ownid/log",
                builder => builder.UseMiddleware<LogMiddleware>());
            routeBuilder.MapMiddlewarePost("not-ownid/register",
                builder => builder.UseMiddleware<ExternalRegisterMiddleware>());
            app.UseRouter(routeBuilder.Build());

            app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
                .AddStrictTransportSecurityMaxAgeIncludeSubDomains()
                .AddContentTypeOptionsNoSniff());
            app.UseOwnId();
        }

        private void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var elasticSection = Configuration.GetSection("ElasticConfiguration");
            var elasticLoggingEnabled = Convert.ToBoolean(elasticSection["Enabled"]);

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Debug()
                .WriteTo.Console();

            if (elasticLoggingEnabled)
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                logger.WriteTo.Elasticsearch(ConfigureElasticSink(elasticSection, environment))
                    .Enrich.WithProperty("source", "net-core-3-sdk")
                    .Enrich.WithProperty("version", version);
            }

            Log.Logger = logger.Enrich.WithProperty("environment", environment)
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
        }

        private ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationSection configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["Uri"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat =
                    $"ownid-{(environment ?? "development").ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                ModifyConnectionSettings = x =>
                    x.BasicAuthentication(configuration["Username"], configuration["Password"]),
                InlineFields = true,
                OverwriteTemplate = true,
                CustomFormatter = new OwnIdFormatter()
            };
        }
    }
}