using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;
using OwnIdSdk.NetCore3.Web.Features;

namespace OwnIdSdk.NetCore3.Web.Configuration
{
    public class OwnIdConfiguration
    {
        private readonly IReadOnlyDictionary<Type, IFeatureConfiguration> _features;

        public OwnIdConfiguration([NotNull] IReadOnlyDictionary<Type, IFeatureConfiguration> features)
        {
            _features = features;
        }

        public IEnumerable<IFeatureConfiguration> Features => _features.Values;

        public TFeature FindFeature<TFeature>() where TFeature : class, IFeatureConfiguration
        {
            return _features.TryGetValue(typeof(TFeature), out var feature) ? (TFeature)feature : null;
        }
        
        public OwnIdConfiguration WithFeature<TFeature>([NotNull] TFeature feature) where TFeature : class, IFeatureConfiguration
        {
            var extensions = Features.ToDictionary(p => p.GetType(), p => p);
            extensions[typeof(TFeature)] = feature;

            return new OwnIdConfiguration(extensions);
        }

        public void Validate()
        {
            if (!_features.ContainsKey(typeof(CoreFeature)))
                throw new InvalidOperationException($"{nameof(CoreFeature)} should be added for any SDK usage");

            if (!_features.ContainsKey(typeof(UserHandlerFeature)))
                throw new InvalidOperationException(
                    $"{nameof(UserHandlerFeature)} should be added to authorize users and updating their profiles");
            
            if (!_features.ContainsKey(typeof(CacheStoreFeature)))
                throw new InvalidOperationException(
                    $"{nameof(CacheStoreFeature)} should be added to store challenge context");
            
            if (!_features.ContainsKey(typeof(LocalizationFeature)))
                throw new InvalidOperationException(
                    $"{nameof(LocalizationFeature)} should be added");

            foreach (var feature in _features.Values)
            {
                feature.Validate();
            }
        }

        public OwnIdConfiguration IntegrateFeatures(IServiceCollection serviceCollection)
        {
            foreach (var feature in _features.Values)
            {
                feature.ApplyServices(serviceCollection);
            }

            return this;
        }
    }
}