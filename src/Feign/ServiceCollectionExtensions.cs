using Feign;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceCollectionExtensions
    {

        public static IFeignBuilder AddFeignClients(this IServiceCollection services)
        {
            return AddFeignClients(services, (FeignOptions)null);
        }

        public static IFeignBuilder AddFeignClients(this IServiceCollection services, Action<FeignOptions> setupAction)
        {
            FeignOptions options = new FeignOptions();
            setupAction?.Invoke(options);
            return AddFeignClients(services, options);
        }

        public static IFeignBuilder AddFeignClients(this IServiceCollection services, FeignOptions options)
        {
            if (options == null)
            {
                options = new FeignOptions();
            }

            FeignBuilder.Instance.Services = services;
            FeignBuilder.Instance.Options = options;

            if (options.Assemblies.Count == 0)
            {
                AddFeignClients(FeignBuilder.Instance.FeignClientTypeBuilder, services, Assembly.GetEntryAssembly(), options.Lifetime);
            }
            else
            {
                foreach (var assembly in options.Assemblies)
                {
                    AddFeignClients(FeignBuilder.Instance.FeignClientTypeBuilder, services, assembly, options.Lifetime);
                }
            }
            FeignBuilder.Instance.FeignClientTypeBuilder.FinishBuild();
            return FeignBuilder.Instance;
        }

        static void AddFeignClients(FeignClientTypeBuilder feignClientTypeBuilder, IServiceCollection services, Assembly assembly, ServiceLifetime lifetime)
        {
            if (assembly == null)
            {
                return;
            }
            foreach (var serviceType in assembly.GetTypes().Where(FeignClientTypeBuilder.NeedBuildType))
            {
                Type proxyType = feignClientTypeBuilder.BuildType(serviceType);
                switch (lifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.TryAddSingleton(serviceType, proxyType);
                        break;
                    case ServiceLifetime.Scoped:
                        services.TryAddScoped(serviceType, proxyType);
                        break;
                    case ServiceLifetime.Transient:
                        services.TryAddTransient(serviceType, proxyType);
                        break;
                    default:
                        break;
                }

            }
        }

    }
}
