using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class Program
    {

        private static IConfigurationRoot Configuration;
        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            try
            {
                ConfigureLogging();
                CreateHostBuilder(args).Build().Run();
            }
            catch (System.Exception ex)
            {
                Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();

        }

        private static void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var elasticSection = Configuration.GetSection("ElasticConfiguration");
            var elasticLoggingEnabled = Convert.ToBoolean(elasticSection["Enabled"]);

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Debug()
                .WriteTo.Console();

            Console.WriteLine($"ELK_LOGGING_ENABLED is {elasticLoggingEnabled.ToString()}");

            if (elasticLoggingEnabled)
            {
                logger.WriteTo.Elasticsearch(ConfigureElasticSink(elasticSection, environment));
            }

            Log.Logger = logger.Enrich.WithProperty("Environment", environment)
            .ReadFrom.Configuration(Configuration)
            .CreateLogger();
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationSection configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["Uri"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                ModifyConnectionSettings = x => x.BasicAuthentication(configuration["Username"], configuration["Password"]),
            };
        }

    }
}