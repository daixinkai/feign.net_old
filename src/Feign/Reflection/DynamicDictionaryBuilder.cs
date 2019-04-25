using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Feign.Reflection
{
    static class DynamicDictionaryBuilder<TSource, TKey, TValue>
    {
        static readonly Func<TSource, IDictionary<TKey, TValue>> _handler = CreateHandler();

        public static IDictionary<TKey, TValue> Build(TSource source)
        {
            return _handler?.Invoke(source);
        }

        static Func<TSource, IDictionary<TKey, TValue>> CreateHandler()
        {
            return null;
            //Expression
            //TypeBuilder t;
        }

    }
}
