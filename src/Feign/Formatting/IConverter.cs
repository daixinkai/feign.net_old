using System;
using System.Collections.Generic;
using System.Text;

namespace Feign.Formatting
{

    public interface IConverter
    {
    }

    public interface IConverter<TSource, TResult> : IConverter
    {
        TResult Convert(TSource value);
    }
}
