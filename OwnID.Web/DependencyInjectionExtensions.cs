using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OwnID.Web
{
    public static class DependencyInjectionExtensions
    {
        public static void TryAddImplementors<T>(this IServiceCollection services,
            IEnumerable<Assembly> assemblies = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            assemblies ??= new[] {typeof(T).Assembly};

            var injectableTypes =
                assemblies.SelectMany(a =>
                    a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)) && !x.IsAbstract));

            foreach (var type in injectableTypes)
                services.TryAdd(new ServiceDescriptor(type, type, lifetime));
        }
    }
}