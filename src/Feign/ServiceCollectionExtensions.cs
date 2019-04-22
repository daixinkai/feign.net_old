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
        public static void AddFeignClients(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
            {
                return;
            }
            FeignClientTypeBuilder feignClientTypeBuilder = new FeignClientTypeBuilder();
            foreach (var assembly in assemblies)
            {
                AddFeignClients(feignClientTypeBuilder, services, assembly);
            }
            feignClientTypeBuilder.FinishBuild();
        }

        public static void AddFeignClients(this IServiceCollection services, params Assembly[] assemblies)
        {
            AddFeignClients(services, assemblies?.AsEnumerable());
        }

        public static void AddFeignClients(this IServiceCollection services)
        {
            AddFeignClients(services, Assembly.GetEntryAssembly());
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
