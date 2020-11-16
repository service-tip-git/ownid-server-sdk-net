using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Web.Extensibility;

namespace OwnIdSdk.NetCore3.Web.Features
{
    public class MetricsFeature : IFeatureConfiguration
    {
        private Action<IServiceCollection> _applyServicesAction;

        public void ApplyServices(IServiceCollection services)
        {
            _applyServicesAction(services);
        }

        public IFeatureConfiguration FillEmptyWithOptional()
        {
            return this;
        }

        public void Validate()
        {
        }

        public MetricsFeature UseMetrics<TMetricsService>()
            where TMetricsService : class, IMetricsService
        {
            _applyServicesAction = services => { services.TryAddSingleton<IMetricsService, TMetricsService>(); };

            return this;
        }
    }
}