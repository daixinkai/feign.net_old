using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Feign.Formatting
{
    public sealed class ConverterCollection : IEnumerable<IConverter>
    {

        System.Collections.Concurrent.ConcurrentDictionary<(Type, Type), IConverter> _map = new System.Collections.Concurrent.ConcurrentDictionary<(Type, Type), IConverter>();


        public IEnumerator<IConverter> GetEnumerator()
        {
            return _map.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void AddConverter<TSource, TResult>(IConverter<TSource, TResult> converter)
        {
            var key = (typeof(TSource), typeof(TResult));
            if (_map.ContainsKey(key))
            {
                _map[key] = converter;
            }
            else
            {
                _map.TryAdd(key, converter);
            }
        }

        public IConverter<TSource, TResult> FindConverter<TSource, TResult>()
        {
            IConverter converter;
            _map.TryGetValue((typeof(TSource), typeof(TResult)), out converter);
            return converter == null ? null : (IConverter<TSource, TResult>)converter;
        }

    }
}
