using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Reflection
{
    class FeignClientProxyServiceEmitMethodBuilder : IMethodBuilder
    {

        static readonly MethodInfo _replacePathVariableMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.IsGenericMethod && o.Name == "ReplacePathVariable");

        static readonly MethodInfo _replaceRequestParamMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.IsGenericMethod && o.Name == "ReplaceRequestParam");

        static readonly MethodInfo _replaceRequestQueryMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.IsGenericMethod && o.Name == "ReplaceRequestQuery");

        public void BuildMethod(MethodInfo method, MethodBuilder methodBuilder)
        {
            BuildMethod(method, methodBuilder, method.GetCustomAttribute<RequestMappingBaseAttribute>());
        }

        public void BuildMethod(MethodInfo method, MethodBuilder methodBuilder, RequestMappingBaseAttribute requestMapping)
        {
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();

            if (requestMapping == null)
            {
                iLGenerator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(Type.EmptyTypes));
                iLGenerator.Emit(OpCodes.Throw);
                return;
            }

            string uri = requestMapping.Value ?? "";

            LocalBuilder local_Uri = iLGenerator.DeclareLocal(typeof(string));
            LocalBuilder local_OldValue = iLGenerator.DeclareLocal(typeof(string));


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
                MethodInfo replaceValueMethod;
                string name;
                if (parameterInfo.IsDefined(typeof(RequestParamAttribute)))
                {
                    name = parameterInfo.GetCustomAttribute<RequestParamAttribute>().Name ?? parameterInfo.Name;
                    replaceValueMethod = _replaceRequestParamMethod;
                }
                else if (parameterInfo.IsDefined(typeof(RequestQueryAttribute)))
                {
                    name = parameterInfo.Name;
                    replaceValueMethod = _replaceRequestQueryMethod;
                }
                else
                {
                    name = parameterInfo.IsDefined(typeof(PathVariableAttribute)) ? parameterInfo.GetCustomAttribute<PathVariableAttribute>().Name : parameterInfo.Name;
                    replaceValueMethod = _replacePathVariableMethod;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = parameterInfo.Name;
                }

                iLGenerator.Emit(OpCodes.Ldstr, name);
                iLGenerator.Emit(OpCodes.Stloc, local_OldValue);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldloc, local_Uri);
                iLGenerator.Emit(OpCodes.Ldloc, local_OldValue);
                iLGenerator.Emit(OpCodes.Ldarg_S, index);

                replaceValueMethod = replaceValueMethod.MakeGenericMethod(parameterInfo.ParameterType);
                iLGenerator.Emit(OpCodes.Call, replaceValueMethod);
                iLGenerator.Emit(OpCodes.Stloc, local_Uri);
                index++;

            }


            var invokeMethod = GetInvokeMethod(method, requestMapping);

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldloc, local_Uri);
            if (NeedRequestBody(invokeMethod))
            {
                if (requestBodyParameter != null)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_S, requestBodyParameterIndex);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldnull);
                }
            }

            iLGenerator.Emit(OpCodes.Call, invokeMethod);


            if (method.ReturnType == null || method.ReturnType == typeof(void))
            {
                LocalBuilder local_Result = iLGenerator.DeclareLocal(invokeMethod.ReturnType);
                iLGenerator.Emit(OpCodes.Stloc, local_Result);
                iLGenerator.Emit(OpCodes.Ldloc, local_Result);
                iLGenerator.Emit(OpCodes.Pop);
            }

            iLGenerator.Emit(OpCodes.Ret);
        }

        MethodInfo GetInvokeMethod(MethodInfo method, RequestMappingBaseAttribute requestMapping)
        {
            Type returnType = GetReturnType(method);
            if (IsTaskMethod(method))
            {
                return GetInvokeMethod(requestMapping, returnType, true);
            }
            return GetInvokeMethod(requestMapping, returnType, false);
        }

        MethodInfo GetInvokeMethod(RequestMappingBaseAttribute requestMapping, Type returnType, bool async)
        {
            MethodInfo httpClientMethod;
            switch (requestMapping.GetMethod()?.ToUpper() ?? "")
            {
                case "GET":
                    httpClientMethod = async ? FeignClientProxyService.HTTP_GET_ASYNC_METHOD : FeignClientProxyService.HTTP_GET_METHOD;
                    break;
                case "POST":
                    httpClientMethod = async ? FeignClientProxyService.HTTP_POST_ASYNC_METHOD : FeignClientProxyService.HTTP_POST_METHOD;
                    break;
                case "PUT":
                    httpClientMethod = async ? FeignClientProxyService.HTTP_PUT_ASYNC_METHOD : FeignClientProxyService.HTTP_PUT_METHOD;
                    break;
                case "DELETE":
                    httpClientMethod = async ? FeignClientProxyService.HTTP_DELETE_ASYNC_METHOD : FeignClientProxyService.HTTP_DELETE_METHOD;
                    break;
                default:
                    throw new ArgumentException("httpMethod error");
            }
            if (returnType == null || returnType == typeof(void))
            {
                return httpClientMethod.MakeGenericMethod(typeof(Newtonsoft.Json.Linq.JObject));
            }
            return httpClientMethod.MakeGenericMethod(returnType);
        }

        bool NeedRequestBody(MethodInfo method)
        {
            return method.GetParameters().Length == 2;
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
