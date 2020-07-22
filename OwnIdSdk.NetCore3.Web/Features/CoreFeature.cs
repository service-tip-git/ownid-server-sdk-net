using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Flow;
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
            
            services.TryAddSingleton<IUrlProvider, UrlProvider>();
            services.TryAddSingleton<FlowController>();
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            _configuration.OwnIdApplicationUrl ??= new Uri(Constants.OwinIdApplicationAddress);

            if (_configuration.CacheExpirationTimeout == default)
                _configuration.CacheExpirationTimeout = (uint) TimeSpan.FromMinutes(10).TotalMilliseconds;
            
            if (_configuration.JwtExpirationTimeout == default)
                _configuration.JwtExpirationTimeout = (uint) TimeSpan.FromMinutes(60).TotalMilliseconds;

            if (_configuration.PollingInterval == default)
                _configuration.PollingInterval = 2000;

            if (_configuration.MaximumNumberOfConnectedDevices == default)
                _configuration.MaximumNumberOfConnectedDevices = 5;

            return this;
        }

        public void Validate()
        {
            // TODO refactor
            var validator = new OwnIdCoreConfigurationValidator();
            var result = validator.Validate(string.Empty, _configuration);

            if (result.Failed)
                throw new InvalidOperationException(result.FailureMessage);
        }
    }
}