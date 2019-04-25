using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public sealed class PathVariableAttribute : RequestParameterBaseAttribute
    {
        public PathVariableAttribute()
        {
        }
        public PathVariableAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
