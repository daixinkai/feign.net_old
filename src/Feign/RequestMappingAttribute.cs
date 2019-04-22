using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class RequestMappingAttribute : Attribute
    {
        public RequestMappingAttribute() { }
        public RequestMappingAttribute(string value) : this(value, "GET")
        {
        }
        public RequestMappingAttribute(string value, string method)
        {
            Value = value;
            Method = method;
        }
        public string Value { get; set; }

        public string Method { get; set; }
    }
}
