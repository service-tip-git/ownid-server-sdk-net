using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OwnIdSdk.NetCore3.Web;
using OwnIdSdk.NetCore3.Web.Gigya;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class Startup
    {
        private const string CorsPolicyName = "_defaultPolicy";

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

            services.AddCors(x =>
            {
                x.AddPolicy(CorsPolicyName, builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddOwnId(
                builder =>
                {
                    builder.UseGigya(gigyaSection["data_center"], gigyaSection["api_key"], gigyaSection["secret"]);
                    builder.SetKeys(ownIdSection["pub_key"], ownIdSection["private_key"]);
                    
                    builder.UseWebCacheStore();

                    builder.WithBaseSettings(x =>
                    {
                        x.DID = ownIdSection["did"];
                        x.Name = ownIdSection["name"];
                        x.Description = ownIdSection["description"];
                        x.Icon = ownIdSection["icon"];
                        x.CallbackUrl = new Uri(ownIdSection["callback_url"]);
                        x.CacheExpirationTimeout = ownIdSection.GetValue("cache_expiration", (uint)TimeSpan.FromMinutes(10).TotalMilliseconds);

                        //for development cases
                        x.IsDevEnvironment = Configuration.GetValue("OwnIdDevelopmentMode", false);
                        x.OwnIdApplicationUrl = new Uri(ownIdSection["web_app_url"] ?? "https://sign.ownid.com");
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
            routeBuilder.MapMiddlewarePost("log",
                builder => builder.UseMiddleware<LogMiddleware>());
            app.UseRouter(routeBuilder.Build());

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
                logger.WriteTo.Elasticsearch(ConfigureElasticSink(elasticSection, environment))
                    .Enrich.WithProperty("source", "net-core-3-sdk");
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
                    $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{(environment ?? "development").ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                ModifyConnectionSettings = x =>
                    x.BasicAuthentication(configuration["Username"], configuration["Password"]),
                InlineFields = true,
                OverwriteTemplate = true,
                CustomFormatter = new OwnIdFormatter()
            };
        }
    }
}