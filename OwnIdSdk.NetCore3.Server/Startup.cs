using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web;

namespace OwnIdSdk.NetCore3.Server
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
            
            app.UseCors(CorsPolicyName);
            // Serve wwwroot/dist as a root 
            // app.UseStaticFiles(new StaticFileOptions() 
            // {
            //     FileProvider = new PhysicalFileProvider(
            //         Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot"))
            // });
            // app.UseHttpsRedirection();
            
            app.UseOwnId();

            // app.UseRouting();
            //
            // app.UseAuthorization();
            //
            // app.UseEndpoints(endpoints => { endpoints.MapRazorPages(); });
        }
    }
}