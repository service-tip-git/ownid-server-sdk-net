using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OwnIdSdk.NetCore3.Configuration;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Features;

namespace OwnIdSdk.NetCore3.Web.Configuration
{
    public class OwnIdConfigurationBuilder : IExtendableConfigurationBuilder
    {
        public OwnIdConfigurationBuilder(IServiceCollection services) : this(
            new OwnIdConfiguration(new Dictionary<Type, IFeatureConfiguration>()))
        {
            Services = services;
            AddOrUpdateFeature(new CoreFeature().FillEmptyWithOptional());
            AddOrUpdateFeature(new LocalizationFeature());
        }

        public OwnIdConfigurationBuilder(OwnIdConfiguration configuration)
        {
            Configuration = configuration;
        }

        public OwnIdConfiguration Configuration { get; private set; }
        
        public IServiceCollection Services { get; }

        public void ModifyBaseSettings(Action<IOwnIdCoreConfiguration> modifyAction)
        {
            WithFeature<CoreFeature>(x => x.WithConfiguration(modifyAction));
        }

        public void SetKeys(string publicKeyPath, string privateKeyPath)
        {
            WithFeature<CoreFeature>(x => x.WithKeys(publicKeyPath, privateKeyPath));
        }

        public void SetKeys(RSA rsa)
        {
            WithFeature<CoreFeature>(x => x.WithKeys(rsa));
        }

        public void SetLocalizationResource(Type resourceType, string resourceName)
        {
            WithFeature<LocalizationFeature>(x => x.WithResource(resourceType, resourceName));
        }

        public void SetStringLocalizer<TLocalizer>() where TLocalizer : IStringLocalizer
        {
            WithFeature<LocalizationFeature>(x => x.WithStringLocalizer<TLocalizer>());
        }

        public void UseUserHandlerWithCustomProfile<TProfile, THandler>()
            where THandler : class, IUserHandler<TProfile> where TProfile : class
        {
            var handlerFeature = Configuration.FindFeature<UserHandlerFeature>() ?? new UserHandlerFeature();
            var builder = WithFeature<CoreFeature>(x =>
                x.WithConfiguration(b =>
                {
                    ((OwnIdCoreConfiguration) b).SetProfileModel<TProfile>();
                    b.ProfileConfiguration.BuildMetadata();
                })
            );
            builder.AddOrUpdateFeature(handlerFeature.UseHandler<TProfile, THandler>());
        }

        public void UseCacheStore<TStore>(ServiceLifetime serviceLifetime) where TStore : class, ICacheStore
        {
            WithFeature<CacheStoreFeature>(x => x.UseStore<TStore>(serviceLifetime));
        }

        public void UseInMemoryCacheStore()
        {
            WithFeature<CacheStoreFeature>(x => x.UseStoreInMemoryStore());
        }

        public void AddOrUpdateFeature<TFeature>([NotNull] TFeature feature)
            where TFeature : class, IFeatureConfiguration
        {
            Configuration = Configuration.WithFeature(feature);
        }

        private OwnIdConfigurationBuilder WithFeature<TFeature>(Func<TFeature, TFeature> setupFunc)
            where TFeature : class, IFeatureConfiguration, new()
        {
            AddOrUpdateFeature(setupFunc(Configuration.FindFeature<TFeature>() ??
                                         new TFeature()));
            return this;
        }
    }
}