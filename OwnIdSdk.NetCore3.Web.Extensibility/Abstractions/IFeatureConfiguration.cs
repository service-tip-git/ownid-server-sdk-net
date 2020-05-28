using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IFeatureConfiguration
    {
        void ApplyServices([NotNull] IServiceCollection services);

        IFeatureConfiguration FillEmptyWithOptional();

        void Validate();
    }
}