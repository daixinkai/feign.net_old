using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Feign.Internal
{
    internal interface IMethodBuilder
    {
        void BuildMethod(MethodInfo method, MethodBuilder methodBuilder);
    }
}
