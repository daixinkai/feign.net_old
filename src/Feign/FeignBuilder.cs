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
            Converters = new ConverterCollection();
            Converters.AddConverter(new ObjectStringConverter());
        }

        public static readonly FeignBuilder Instance = new FeignBuilder();

        public ConverterCollection Converters { get; }

        public IServiceCollection Services { get; set; }

    }
}
