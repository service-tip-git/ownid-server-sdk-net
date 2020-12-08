using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnID.Configuration;
using OwnID.Extensibility.Configuration;
using OwnID.Flow.Commands.MagicLink;
using OwnID.Web.Attributes;
using OwnID.Web.Extensibility;

namespace OwnID.Web.Features
{
    [FeatureDependency(typeof(CoreFeature))]
    public class MagicLinkFeature : IFeatureConfiguration
    {
        private readonly IMagicLinkConfiguration _configuration;

        public MagicLinkFeature()
        {
            _configuration = new MagicLinkConfiguration();
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.TryAddSingleton<SendMagicLinkCommand>();
            services.TryAddSingleton<ExchangeMagicLinkCommand>();
            services.TryAddSingleton(_configuration);
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            if (_configuration.TokenLifetime == default)
                _configuration.TokenLifetime = (uint) TimeSpan.FromMinutes(10).TotalMilliseconds;

            return this;
        }

        public void Validate()
        {
            if (!OwnIdCoreConfigurationValidator.IsUriValid($"MagicLink.{nameof(_configuration.RedirectUrl)}",
                _configuration.RedirectUrl, true, out var errMessage))
                throw new InvalidOperationException(errMessage);
        }

        public MagicLinkFeature WithConfiguration(Action<IMagicLinkConfiguration> setupAction)
        {
            setupAction(_configuration);
            return this;
        }
    }
}