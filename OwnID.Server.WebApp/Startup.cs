using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace OwnID.Server.WebApp
{
    public class Startup
    {
        private const string CorsPolicyName = "AllowAll";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var webAppOptions = Configuration.GetSection("WebApp").Get<WebAppOptions>() ?? WebAppOptions.Default;

            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy(CorsPolicyName, builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowCredentials();

                    builder.SetIsOriginAllowedToAllowWildcardSubdomains();

                    var originsList = new List<string>();

                    if (webAppOptions.IsDevEnvironment)
                    {
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
                    }
                    else
                    {
                        originsList.Add($"https://*.{webAppOptions.TopDomain}");
                        originsList.Add($"https://{webAppOptions.TopDomain}");
                    }

                    builder.WithOrigins(originsList.ToArray());
                });
            });

            services.AddOptions();
            services.Configure<WebAppOptions>(Configuration.GetSection(WebAppOptions.ConfigurationName));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "OwnID.Server.WebApp", Version = "v1"});

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OwnID.Server.WebApp v1"));
            }

            app.UseCors(CorsPolicyName);

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}