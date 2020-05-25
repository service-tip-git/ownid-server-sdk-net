using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using OwnIdSdk.NetCore3.Server.Gigya.Resources;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; set; }

        private const string CorsPolicyName = "AllowAll";
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            
            services.AddCors(x =>
            {
                x.AddPolicy(CorsPolicyName , builder =>
                {
                    // builder.AllowCredentials();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            });

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            
            var ownIdSection = Configuration.GetSection("ownid");
            services.AddOwnId<UserProfile, ClientAppChallengeHandler, InMemoryCacheStore>(x =>
            {
                x.SetKeysFromFiles(ownIdSection["pub_key"], ownIdSection["private_key"]);
                x.CallbackUrl = new Uri(ownIdSection["callback_url"]);
                x.OwnIdApplicationUrl = new Uri(ownIdSection["web_app_url"]);
                x.Requester.DID = ownIdSection["did"];
                x.Requester.Name = ownIdSection["name"];
                x.Requester.Description = ownIdSection["description"];
                x.Requester.Icon = ownIdSection["icon"];
                x.IsDevEnvironment = Environment.IsDevelopment();
                x.LocalizationResourceType = typeof(Server_Gigya);
                x.LocalizationResourceName = "Server.Gigya";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Environment = env;
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
            app.UseOwnId();
        }
    }
}