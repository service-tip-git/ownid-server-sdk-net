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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (var publicKeyReader = File.OpenText(@"./Keys/jwtRS256.key.pub"))
            using (var privateKeyReader = File.OpenText(@"./Keys/jwtRS256.key"))   
            {
                services.AddOwnId<SampleChallengeHandler, InMemoryCacheStore>(new ProviderConfiguration(
                    RsaHelper.ReadKeyFromPem(publicKeyReader),
                    RsaHelper.ReadKeyFromPem(privateKeyReader),
                    "http://a52b6bdc8e3954bdf8b129e49fc4fa0f-1986585172.us-east-2.elb.amazonaws.com",
                    new List<ProfileField>
                    {
                        ProfileField.Email,
                        ProfileField.FirstName,
                        ProfileField.LastName
                    }, 
                    "http://localhost:5000/",
                    new Requester
                    {
                        DID = "ownid:did:123123123",
                        Name = "mozambiquehe.re",
                        Description = "My description"
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
            
            app.UseOwnId();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}