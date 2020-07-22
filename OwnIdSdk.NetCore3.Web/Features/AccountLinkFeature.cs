using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Web.Extensibility;
using OwnIdSdk.NetCore3.Web.FlowEntries;
using OwnIdSdk.NetCore3.Web.FlowEntries.Adapters;

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

        public AccountLinkFeature UseAccountLinking<TProfile, THandler>()
            where THandler : class, IAccountLinkHandler<TProfile> where TProfile : class
        {
            _applyServicesAction = services =>
            {
                services.TryAddTransient<IAccountLinkHandler<TProfile>, THandler>();
                services.TryAddTransient<IAccountLinkHandlerAdapter, AccountLinkHandlerAdapter<TProfile>>();
            };

            return this;
        }
    }
}