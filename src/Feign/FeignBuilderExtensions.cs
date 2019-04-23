using Feign;
using Feign.Formatting;
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
    }
}
