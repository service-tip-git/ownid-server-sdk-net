using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Web.Extensibility;

namespace OwnID.Web.Features
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