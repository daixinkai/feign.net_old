using Feign;
using Feign.Discovery;
using Feign.Formatting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FeignBuilderExtensions
    {
        public static IFeignBuilder AddConverter<TSource, TResult>(this IFeignBuilder feignBuilder, IConverter<TSource, TResult> converter)
        {
            feignBuilder.Converters.AddConverter(converter);
            return feignBuilder;
        }

        public static IFeignBuilder AddServiceDiscovery(this IFeignBuilder feignBuilder, IServiceDiscovery serviceDiscovery)
        {
            feignBuilder.Services.TryAddSingleton(serviceDiscovery);
            return feignBuilder;
        }

        public static IFeignBuilder AddServiceDiscovery<T>(this IFeignBuilder feignBuilder) where T : class, IServiceDiscovery
        {
            feignBuilder.Services.TryAddSingleton<IServiceDiscovery, T>();
            return feignBuilder;
        }

    }
}
