using Feign.Discovery;
using Feign.Internal;
using Feign.Proxy;
using Microsoft.Extensions.DependencyInjection;
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
            ConstructorInfo baseConstructorInfo = typeof(FeignClientProxyService).GetConstructors()[0];

            var parameters = baseConstructorInfo.GetParameters();

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
               MethodAttributes.Public,
               CallingConventions.Standard,
           parameters.Select(p => p.ParameterType).ToArray());

            ILGenerator constructorIlGenerator = constructorBuilder.GetILGenerator();
            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            for (int i = 1; i <= parameters.Length; i++)
            {
                constructorIlGenerator.Emit(OpCodes.Ldarg_S, i);
            }
            constructorIlGenerator.Emit(OpCodes.Call, baseConstructorInfo);
            constructorIlGenerator.Emit(OpCodes.Ret);
        }

        void BuildReadOnlyProperty(TypeBuilder typeBuilder, Type interfaceType, string propertyName, string propertyValue)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, typeof(string), Type.EmptyTypes);

            //if (property.CanRead)
            //{
            MethodBuilder propertyGet = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), Type.EmptyTypes);
            ILGenerator iLGenerator = propertyGet.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldstr, propertyValue);
            iLGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(propertyGet);
            //}
            //if (property.CanWrite)
            //{
            //    MethodBuilder propertySet = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(string), Type.EmptyTypes);
            //    ILGenerator iLGenerator = propertySet.GetILGenerator();
            //    iLGenerator.Emit(OpCodes.Ldstr, interfaceType.GetCustomAttribute<FeignClientAttribute>().Name);
            //    iLGenerator.Emit(OpCodes.Ret);
            //    propertyBuilder.SetSetMethod(propertySet);
            //}

        }

        void BuildServiceIdProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            BuildReadOnlyProperty(typeBuilder, interfaceType, "ServiceId", interfaceType.GetCustomAttribute<FeignClientAttribute>().Name);
        }

        void BuildBaseUriProperty(TypeBuilder typeBuilder, Type interfaceType)
        {
            BuildReadOnlyProperty(typeBuilder, interfaceType, "BaseUri", interfaceType.GetCustomAttribute<RequestMappingAttribute>().Value);
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
