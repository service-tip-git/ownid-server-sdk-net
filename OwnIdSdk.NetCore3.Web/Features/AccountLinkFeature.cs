using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Web.Extensibility;

namespace OwnIdSdk.NetCore3.Web.Features
{
    public class AccountLinkFeature : IFeatureConfiguration
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

        public AccountLinkFeature UseAccountLinking<THandler>() where THandler : class, IAccountLinkHandler
        {
            _applyServicesAction = services => { services.TryAddTransient<IAccountLinkHandler, THandler>(); };

            return this;
        }
    }
}