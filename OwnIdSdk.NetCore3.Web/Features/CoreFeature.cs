using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Features
{
    public class CoreFeature : IFeatureConfiguration
    {
        private readonly OwnIdCoreConfiguration _configuration;

        public CoreFeature()
        {
            _configuration = new OwnIdCoreConfiguration();
        }

        public CoreFeature WithConfiguration(Action<IOwnIdCoreConfiguration> setupAction)
        {
            setupAction(_configuration);
            return this;
        }

        public CoreFeature WithKeys(string publicKeyPath, string privateKeyPath)
        {
            using var publicKeyReader = File.OpenText(publicKeyPath);
            using var privateKeyReader = File.OpenText(privateKeyPath);
            _configuration.JwtSignCredentials = RsaHelper.LoadKeys(publicKeyReader, privateKeyReader);
            return this;
        }
        
        public CoreFeature WithKeys(RSA rsa)
        {
            _configuration.JwtSignCredentials = rsa;
            return this;
        }
        
        public void ApplyServices(IServiceCollection services)
        {
            services.TryAddSingleton(x => (IOwnIdCoreConfiguration) _configuration);
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            _configuration.OwnIdApplicationUrl ??= new Uri(Constants.OwinIdApplicationAddress);

            return this;
        }

        public void Validate()
        {
            // TODO refactor
            var validator = new OwnIdConfigurationValidator();
            var result = validator.Validate(string.Empty, _configuration);

            if (result.Failed)
                throw new InvalidOperationException(result.FailureMessage);
        }
    }
}