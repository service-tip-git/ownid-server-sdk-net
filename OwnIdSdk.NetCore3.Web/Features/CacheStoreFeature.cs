using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Store;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.Store;

namespace OwnIdSdk.NetCore3.Web.Features
{
    public class CacheStoreFeature : IFeatureConfiguration
    {
        private Type _storeType;
        private ServiceLifetime _serviceLifetime;
        private Action<IServiceCollection> _servicesInitialization;
        
        public CacheStoreFeature UseStoreInMemoryStore()
        {
            _storeType = typeof(InMemoryCacheStore);
            _serviceLifetime = ServiceLifetime.Singleton;
            return this;
        }

        public CacheStoreFeature UseWebCacheStore()
        {
            _servicesInitialization = 
                services => services.AddMemoryCache();
            
            _storeType = typeof(WebCacheStore);
            _serviceLifetime = ServiceLifetime.Singleton;
            
            return this;
        }

        public CacheStoreFeature UseStore<TStore>(ServiceLifetime serviceLifetime) where TStore : class, ICacheStore
        {
            _serviceLifetime = serviceLifetime;
            _storeType = typeof(TStore);
            
            return this;
        }
        
        public void ApplyServices(IServiceCollection services)
        {
            _servicesInitialization?.Invoke(services);
            
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