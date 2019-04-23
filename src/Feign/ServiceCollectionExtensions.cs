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
        public static IFeignBuilder AddFeignClients(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
            {
                return FeignBuilder.Instance;
            }
            FeignClientTypeBuilder feignClientTypeBuilder = new FeignClientTypeBuilder();
            foreach (var assembly in assemblies)
            {
                AddFeignClients(feignClientTypeBuilder, services, assembly);
            }
            feignClientTypeBuilder.FinishBuild();
            FeignBuilder.Instance.Services = services;
            return FeignBuilder.Instance;
        }

        public static IFeignBuilder AddFeignClients(this IServiceCollection services, params Assembly[] assemblies)
        {
            return AddFeignClients(services, assemblies?.AsEnumerable());
        }

        public static IFeignBuilder AddFeignClients(this IServiceCollection services)
        {
            return AddFeignClients(services, Assembly.GetEntryAssembly());
        }

        static void AddFeignClients(FeignClientTypeBuilder feignClientTypeBuilder, IServiceCollection services, Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }
            foreach (var serviceType in assembly.GetTypes().Where(FeignClientTypeBuilder.NeedBuildType))
            {
                services.TryAddTransient(serviceType, feignClientTypeBuilder.BuildType(serviceType));
            }
        }
    }
}
