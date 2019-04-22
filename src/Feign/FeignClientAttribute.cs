using System;

namespace Feign
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class FeignClientAttribute : Attribute
    {
        public FeignClientAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
