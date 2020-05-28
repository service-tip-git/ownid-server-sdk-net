using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility.Abstractions;

namespace OwnIdSdk.NetCore3.Web.Features
{
    public class CacheStoreFeature : IFeatureConfiguration
    {
        private Type _storeType;
        private ServiceLifetime _serviceLifetime;

        public CacheStoreFeature()
        {
        }

        protected CacheStoreFeature(CacheStoreFeature feature)
        {
            _storeType = feature._storeType;
            _serviceLifetime = feature._serviceLifetime;
        }

        public CacheStoreFeature UseStoreInMemoryStore()
        {
            _storeType = typeof(InMemoryCacheStore);
            _serviceLifetime = ServiceLifetime.Singleton;
            return new CacheStoreFeature(this);
        }

        public CacheStoreFeature UseStore<TStore>(ServiceLifetime serviceLifetime) where TStore : class, ICacheStore
        {
            _serviceLifetime = serviceLifetime;
            _storeType = typeof(TStore);
            return new CacheStoreFeature(this);
        }
        
        public void ApplyServices(IServiceCollection services)
        {
            services.TryAdd(new ServiceDescriptor(typeof(ICacheStore), _storeType, _serviceLifetime));
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            if (_storeType == null)
            {
                _storeType = typeof(InMemoryCacheStore);
                _serviceLifetime = ServiceLifetime.Singleton;
            }

            return this;
        }

        public void Validate()
        {
            if(_storeType == null)
                throw new InvalidOperationException("Store Type can not be null");
        }
    }
}