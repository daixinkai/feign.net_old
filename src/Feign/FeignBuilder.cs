using System;
using System.Collections.Generic;
using System.Text;
using Feign.Formatting;
using Microsoft.Extensions.DependencyInjection;

namespace Feign
{
    sealed class FeignBuilder : IFeignBuilder
    {

        public FeignBuilder()
        {
            FeignClientTypeBuilder = new FeignClientTypeBuilder();
        }

        public static readonly FeignBuilder Instance = new FeignBuilder();

        public ConverterCollection Converters { get { return Options?.Converters; } }

        public FeignOptions Options { get; set; }

        public IServiceCollection Services { get; set; }

        public FeignClientTypeBuilder FeignClientTypeBuilder { get; }

    }
}
