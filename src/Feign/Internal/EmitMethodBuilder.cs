using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Internal
{
    class EmitMethodBuilder : IMethodBuilder
    {

        //static readonly MethodInfo _stringReplaceMethod = typeof(String).GetMethod("Replace", new Type[] { typeof(string), typeof(string) });

        #region http method
        static readonly MethodInfo _getMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.Name == "Get");
        static readonly MethodInfo _getAsyncMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.Name == "GetAsync");

        static readonly MethodInfo _postMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.Name == "Post");
        static readonly MethodInfo _postAsyncMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.Name == "PostAsync");

        #endregion

        static readonly MethodInfo _replacePathVariableMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault(o => o.Name == "ReplacePathVariable");


        public void BuildMethod(MethodInfo method, MethodBuilder methodBuilder)
        {
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();

            RequestMappingAttribute requestMapping = method.GetCustomAttribute<RequestMappingAttribute>();

            string uri = requestMapping.Value;

            LocalBuilder local_Uri = iLGenerator.DeclareLocal(typeof(string));
            LocalBuilder local_OldValue = iLGenerator.DeclareLocal(typeof(string));
            LocalBuilder local_Result = iLGenerator.DeclareLocal(method.ReturnType);


            iLGenerator.Emit(OpCodes.Ldstr, uri);
            iLGenerator.Emit(OpCodes.Stloc, local_Uri);

            ParameterInfo requestBodyParameter = null;
            int requestBodyParameterIndex = -1;

            int index = 1;
            foreach (var parameterInfo in method.GetParameters())
            {
                if (parameterInfo.IsDefined(typeof(RequestBodyAttribute)))
                {
                    if (requestBodyParameter != null)
                    {
                        throw new ArgumentException("最多只能有一个RequestBody", parameterInfo.Name);
                    }
                    requestBodyParameter = parameterInfo;
                    requestBodyParameterIndex = index;
                    continue;
                }
                string name = parameterInfo.IsDefined(typeof(PathVariableAttribute)) ? parameterInfo.GetCustomAttribute<PathVariableAttribute>().Name : parameterInfo.Name;
                iLGenerator.Emit(OpCodes.Ldstr, name);
                iLGenerator.Emit(OpCodes.Stloc, local_OldValue);
                iLGenerator.Emit(OpCodes.Ldloc, local_Uri);
                iLGenerator.Emit(OpCodes.Ldloc, local_OldValue);
                iLGenerator.Emit(OpCodes.Ldarg_S, index);
                iLGenerator.Emit(OpCodes.Call, _replacePathVariableMethod);
                iLGenerator.Emit(OpCodes.Stloc, local_Uri);
                index++;
            }


            Type returnType = GetReturnType(method);

            var invokeMethod = GetInvokeMethod(method, requestMapping);

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldloc, local_Uri);
            if (requestBodyParameter != null)
            {
                iLGenerator.Emit(OpCodes.Ldarg_S, requestBodyParameterIndex);
            }
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Stloc, local_Result);
            iLGenerator.Emit(OpCodes.Ldloc, local_Result);


            iLGenerator.Emit(OpCodes.Ret);
        }

        MethodInfo GetInvokeMethod(MethodInfo method, RequestMappingAttribute requestMapping)
        {
            Type returnType = GetReturnType(method);
            if (IsTaskMethod(method))
            {
                return GetInvokeMethod(requestMapping, returnType, true);
            }
            return GetInvokeMethod(requestMapping, returnType, false);
        }

        MethodInfo GetInvokeMethod(RequestMappingAttribute requestMapping, Type returnType, bool async)
        {
            MethodInfo httpClientMethod;
            switch (requestMapping.Method?.ToUpper() ?? "")
            {
                case "GET":
                    httpClientMethod = async ? _getAsyncMethod : _getMethod;
                    break;
                case "POST":
                    httpClientMethod = async ? _postAsyncMethod : _postMethod;
                    break;
                default:
                    throw new ArgumentException("httpMethod error");
            }
            if (returnType == null)
            {
                return httpClientMethod.MakeGenericMethod(typeof(Newtonsoft.Json.Linq.JObject));
            }
            return httpClientMethod.MakeGenericMethod(returnType);
        }


        bool IsTaskMethod(MethodInfo method)
        {
            return method.ReturnType == typeof(Task) || method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        }

        Type GetReturnType(MethodInfo method)
        {
            if (!IsTaskMethod(method))
            {
                return method.ReturnType;
            }

            if (method.ReturnType.IsGenericType)
            {
                return method.ReturnType.GetGenericArguments()[0];
            }

            return method.ReturnType;
        }

    }
}
