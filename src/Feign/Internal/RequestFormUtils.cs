using Feign.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Feign.Internal
{
    static class RequestFormUtils
    {
        public static IDictionary<string, string> ConvertToMap<T>(T value)
        {
            if (value == null)
            {
                return new Dictionary<string, string>();
            }
            if (value is IDictionary<string, string>)
            {
                return (IDictionary<string, string>)value;
            }
            if (value is IEnumerable<KeyValuePair<string, string>>)
            {
                return ((IEnumerable<KeyValuePair<string, string>>)value).ToDictionary(s => s.Key, s => s.Value);
            }

            if (value is IEnumerable)
            {
                return new Dictionary<string, string>();
            }

            if (Type.GetTypeCode(value.GetType()) != TypeCode.Object)
            {
                return new Dictionary<string, string>();
            }

            return DynamicDictionaryBuilder<T, string, string>.Build(value);
        }
    }
}
