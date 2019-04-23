using Feign.Formatting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IFeignBuilder
    {
        ConverterCollection Converters { get; }
        IServiceCollection Services { get; }
    }
}
