using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OwnIdSdk.NetCore3.Web;
using OwnIdSdk.NetCore3.Web.Gigya;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private const string CorsPolicyName = "AllowAll";
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(x =>
            {
                x.AddPolicy(CorsPolicyName, builder =>
                {
                    // builder.AllowCredentials();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            });

            var ownIdSection = Configuration.GetSection("ownid");
            var gigyaSection = Configuration.GetSection("gigya");
            
            services.AddOwnId(
                builder =>
                {
                    builder.UseGigya(gigyaSection["data_center"], gigyaSection["api_key"], gigyaSection["secret"]);
                    builder.SetKeys(ownIdSection["pub_key"], ownIdSection["private_key"]);
                    
                    builder.WithBaseSettings(x =>
                    {
                        x.DID = ownIdSection["did"];
                        x.Name = ownIdSection["name"];
                        x.Description = ownIdSection["description"];
                        x.Icon = ownIdSection["icon"];
                        x.CallbackUrl = new Uri(ownIdSection["callback_url"]);
                            
                        //for development cases
                        x.IsDevEnvironment = Configuration.GetValue("OwnIdDevelopmentMode", false);
                        x.OwnIdApplicationUrl = new Uri(ownIdSection["web_app_url"]);
                    });
                });
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
            app.UseOwnId();
        }
    }
}