using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            using (var publicKeyReader = File.OpenText(ownIdSection["pub_key"]))
            using (var privateKeyReader = File.OpenText(ownIdSection["private_key"]))   
            {
                services.AddOwnId<SampleChallengeHandler, InMemoryCacheStore>(new ProviderConfiguration(
                    RsaHelper.ReadKeyFromPem(publicKeyReader),
                    RsaHelper.ReadKeyFromPem(privateKeyReader),
                    ownIdSection["web_app_url"],
                    new List<ProfileField>
                    {
                        ProfileField.Email,
                        ProfileField.FirstName,
                        ProfileField.LastName
                    }, 
                    ownIdSection["callback_url"],
                    new Requester
                    {
                        DID = ownIdSection["did"],
                        Name = ownIdSection["name"],
                        Description = ownIdSection["description"]
                    }
                ));
            }
            
           
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors(CorsPolicyName);
            app.UseOwnId();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}