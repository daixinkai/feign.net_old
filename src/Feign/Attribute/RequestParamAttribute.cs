using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public sealed class RequestParamAttribute : RequestParameterBaseAttribute
    {
        public RequestParamAttribute()
        {
        }
        public RequestParamAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
