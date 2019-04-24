﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Feign.Internal
{
    static class FeignClientUtils
    {
        #region PathVariable
        public static string ReplacePathVariable(string uri, string name, string value)
        {
            name = "{" + name + "}";
            return uri.Replace(name, value);
        }

        public static string ReplacePathVariable<T>(string uri, string name, T value)
        {
            return ReplacePathVariable(uri, name, ConvertValue<T, string>(value, true));
        }
        #endregion

        #region RequestParam
        public static string ReplaceRequestParam(string uri, string name, string value)
        {
            string pattern = "(&?)" + name + "={" + name + "}(&?)([.^&]*)";
            return Regex.Replace(uri, pattern, match =>
             {
                 return match.Value.Replace("{" + name + "}", value);
             });
        }

        public static string ReplaceRequestParam<T>(string uri, string name, T value)
        {
            return ReplaceRequestParam(uri, name, ConvertValue<T, string>(value, true));
        }
        #endregion

        #region RequestQuery
        public static string ReplaceRequestQuery(string uri, string name, string value)
        {
            if (uri.IndexOf("?") >= 0)
            {
                return uri + $"&{name}={value}";
            }
            else
            {
                return uri + $"?{name}={value}";
            }
        }
        public static string ReplaceRequestQuery<T>(string uri, string name, T value)
        {
            var typeCode = Type.GetTypeCode(typeof(T));
            if (typeCode == TypeCode.Object)
            {
                if (value == null)
                {
                    return uri;
                }
                //TODO: ReplaceRequestQuery
                //foreach (var property in value.GetType().GetProperties())
                //{
                //    object propertyValue = property.GetValue(value);
                //    if (propertyValue == null)
                //    {
                //        continue;
                //    }
                //    if (propertyValue is IEnumerable&&propertyValue)
                //    {

                //    }
                //}
                return uri;
            }
            else
            {
                return ReplaceRequestQuery(uri, name, ConvertValue<T, string>(value, true));
            }
        }
        #endregion


        static TResult ConvertValue<TSource, TResult>(TSource value, bool useDefault)
        {
            var converter = FeignBuilder.Instance.Converters.FindConverter<TSource, TResult>();
            if (converter == null)
            {
                if (!useDefault)
                {
                    return default(TResult);
                }
                return FeignBuilder.Instance.Converters.FindConverter<object, TResult>().Convert(value);
            }
            return converter.Convert(value);
        }

    }
}
