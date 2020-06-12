using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Gigya
{
    public class GigyaIntegrationFeature : IFeatureConfiguration
    {
        private readonly GigyaConfiguration _configuration;

        public GigyaIntegrationFeature()
        {
            _configuration = new GigyaConfiguration();
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.TryAddSingleton(_configuration);
            services.TryAddTransient<GigyaRestApiClient>();
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_configuration.ApiKey) ||
                string.IsNullOrWhiteSpace(_configuration.SecretKey) ||
                string.IsNullOrWhiteSpace(_configuration.DataCenter))
                throw new InvalidOperationException(
                    $"{nameof(_configuration.ApiKey)}, {nameof(_configuration.SecretKey)} and {nameof(_configuration.DataCenter)} should be provided");
        }

        public GigyaIntegrationFeature WithConfig(Action<GigyaConfiguration> configAction)
        {
            configAction(_configuration);
            return this;
        }
    }
}