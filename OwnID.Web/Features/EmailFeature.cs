using System;
using Microsoft.Extensions.DependencyInjection;
using OwnID.Configuration;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Services;
using OwnID.Services;
using OwnID.Web.Extensibility;

namespace OwnID.Web.Features
{
    public class EmailFeature : IFeatureConfiguration
    {
        private readonly ISmtpConfiguration _configuration;

        public EmailFeature()
        {
            _configuration = new SmtpConfiguration();
        }
        
        public EmailFeature WithConfiguration(Action<ISmtpConfiguration> setupAction)
        {
            setupAction(_configuration);
            return this;
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton(_configuration);
            services.AddSingleton<IEmailService, EmailService>();
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            if (_configuration.Port == 0)
                _configuration.Port = 465;

            _configuration.FromName ??= string.Empty;

            return this;
        }

        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(_configuration.FromAddress))
                throw new InvalidOperationException($"Smpt.{nameof(_configuration.FromAddress)} is required");
            
            if(string.IsNullOrWhiteSpace(_configuration.Host))
                throw new InvalidOperationException($"Smpt.{nameof(_configuration.Host)} is required");
        }
    }
}