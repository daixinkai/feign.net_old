using Feign.Internal;
using Feign.Proxy;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Feign
{
    internal class FeignClientTypeBuilder
    {
        public FeignClientTypeBuilder()
        {
            _guid = Guid.NewGuid().ToString("N").ToUpper();
            _suffix = "_Proxy_" + _guid;
            _methodBuilder = new EmitMethodBuilder();
        }

        string _guid;
        string _suffix;

        AssemblyBuilder _assemblyBuilder;
        ModuleBuilder _moduleBuilder;
        IMethodBuilder _methodBuilder;
        void EnsureAssemblyBuilder()
        {
            if (_assemblyBuilder == null)
            {
                _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.Run);
            }
        }
        void EnsureModuleBuilder()
        {
            EnsureAssemblyBuilder();
            if (_moduleBuilder == null)
            {
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
            }
        }

        public Type BuildType(Type interfaceType)
        {
            if (!NeedBuildType(interfaceType))
            {
                return null;
            }
            TypeBuilder typeBuilder = CreateTypeBuilder(interfaceType.FullName + _suffix, typeof(FeignClientProxyService));
            BuildConstructor(typeBuilder);
            BuildServiceIdProperty(typeBuilder, interfaceType);
            BuildBaseUriProperty(typeBuilder, interfaceType);
            typeBuilder.AddInterfaceImplementation(interfaceType);
            foreach (var method in interfaceType.GetMethods())
            {
                MethodBuilder methodBuilder = CreateMethodBuilder(typeBuilder, method);
                //build body
                if (!method.IsDefined(typeof(RequestMappingAttribute)))
                {
                    ILGenerator iLGenerator = methodBuilder.GetILGenerator();
                    iLGenerator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(Type.EmptyTypes));
                    iLGenerator.Emit(OpCodes.Throw);
                    continue;
                }
                _methodBuilder.BuildMethod(method, methodBuilder);
            }
            var typeInfo = typeBuilder.CreateTypeInfo();
            Type type = typeInfo.AsType();
            return type;
        }



        void BuildConstructor(TypeBuilder typeBuilder)
        {
            ConstructorInfo baseConstructorInfo = typeof(FeignClientProxyService).GetConstructor(new Type[] { typeof(IDiscoveryClient) });
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
               MethodAttributes.Public,
               CallingConventions.Standard,
               new Type[] { typeof(IDiscoveryClient) });

            ILGenerator constructorIlGenerator = constructorBuilder.GetILGenerator();
            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            constructorIlGenerator.Emit(OpCodes.Ldarg_1);
            constructorIlGenerator.Emit(OpCodes.Call, baseConstructorInfo);
            constructorIlGenerator.Emit(OpCodes.Ret);
        }

        void BuildServiceIdProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("ServiceId", PropertyAttributes.None, typeof(string), Type.EmptyTypes);
            MethodBuilder propertyGet = typeBuilder.DefineMethod("get_ServiceId", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), Type.EmptyTypes);
            ILGenerator iLGenerator = propertyGet.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldstr, interfaceType.GetCustomAttribute<FeignClientAttribute>().Name);
            iLGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(propertyGet);
        }


        void BuildBaseUriProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            if (!interfaceType.IsDefined(typeof(RequestMappingAttribute)))
            {
                return;
            }
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("BaseUri", PropertyAttributes.None, typeof(string), Type.EmptyTypes);
            MethodBuilder propertyGet = typeBuilder.DefineMethod("get_BaseUri", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), Type.EmptyTypes);
            ILGenerator iLGenerator = propertyGet.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldstr, interfaceType.GetCustomAttribute<RequestMappingAttribute>().Value);
            iLGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(propertyGet);
        }

        string GetServiceId(Type interfaceType)
        {
            return interfaceType.GetCustomAttribute<FeignClientAttribute>().Name;
        }

        MethodBuilder CreateMethodBuilder(TypeBuilder typeBuilder, MethodInfo method)
        {
            MethodAttributes methodAttributes;
            if (method.IsVirtual)
            {
                methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
            }
            else
            {
                methodAttributes = MethodAttributes.Public;
            }
            var arguments = method.GetParameters().Select(a => a.ParameterType).ToArray();
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, methodAttributes, CallingConventions.Standard, method.ReturnType, arguments);
            typeBuilder.DefineMethodOverride(methodBuilder, method);
            return methodBuilder;
        }

        internal static bool NeedBuildType(Type type)
        {
            return type.IsInterface && type.IsDefined(typeof(FeignClientAttribute));
        }

        private TypeBuilder CreateTypeBuilder(string typeName, Type parentType)
        {
            EnsureModuleBuilder();

            return _moduleBuilder.DefineType(typeName,
                          TypeAttributes.Public |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout,
                          parentType);
        }



        public void FinishBuild()
        {
        }

    }
}
