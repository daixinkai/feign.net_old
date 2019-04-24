﻿using Feign.Formatting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Feign
{
    public class FeignOptions
    {
        public FeignOptions()
        {
            Converters = new ConverterCollection();
            Converters.AddConverter(new ObjectStringConverter());
            Assemblies = new List<Assembly>();
            Lifetime = ServiceLifetime.Transient;
        }
        public ConverterCollection Converters { get; }
        public IList<Assembly> Assemblies { get; }
        /// <summary>
        /// default <see cref="ServiceLifetime.Transient"/>
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }
    }
}
