using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IExtendableConfigurationBuilder
    {
        IServiceCollection Services { get; }
        
        void AddOrUpdateFeature<TFeature>([NotNull] TFeature feature) where TFeature : class, IFeatureConfiguration;

        void UseAccountLinking<TProfile, THandler>() where THandler : class, IAccountLinkHandler<TProfile>
            where TProfile : class;

        void UseUserHandlerWithCustomProfile<TProfile, THandler>()
            where THandler : class, IUserHandler<TProfile> where TProfile : class;
    }
}