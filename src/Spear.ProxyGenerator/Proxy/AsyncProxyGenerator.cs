using Spear.ProxyGenerator.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Spear.ProxyGenerator.Proxy
{
    public class AsyncProxyGenerator : IDisposable
    {
        private readonly Dictionary<Type, Dictionary<Type, Type>> _proxyTypeCaches;

        private readonly ProxyAssembly _proxyAssembly;

        private readonly MethodInfo _dispatchProxyInvokeMethod = typeof(ProxyExecutor).GetTypeInfo().GetDeclaredMethod("Invoke");
        private readonly MethodInfo _dispatchProxyInvokeAsyncMethod = typeof(ProxyExecutor).GetTypeInfo().GetDeclaredMethod("InvokeAsync");
        private readonly MethodInfo _dispatchProxyInvokeAsyncTMethod = typeof(ProxyExecutor).GetTypeInfo().GetDeclaredMethod("InvokeAsyncT");

        public AsyncProxyGenerator()
        {
            _proxyTypeCaches = new Dictionary<Type, Dictionary<Type, Type>>();
            _proxyAssembly = new ProxyAssembly();
        }

        /// <summary> 创建代理 </summary>
        /// <param name="interfaceType"></param>
        /// <param name="baseType"></param>
        /// <param name="proxyProvider"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object CreateProxy(Type interfaceType, Type baseType, IProxyProvider proxyProvider, object key = null)
        {
            var proxiedType = GetProxyType(baseType, interfaceType);
            return Activator.CreateInstance(proxiedType, proxyProvider, key, new ProxyHandler(this));
        }

        /// <summary> 获取代理类型/// </summary>
        /// <param name="baseType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        private Type GetProxyType(Type baseType, Type interfaceType)
        {
            lock (_proxyTypeCaches)
            {
                if (!_proxyTypeCaches.TryGetValue(baseType, out var interfaceToProxy))
                {
                    interfaceToProxy = new Dictionary<Type, Type>();
                    _proxyTypeCaches[baseType] = interfaceToProxy;
                }

                if (!interfaceToProxy.TryGetValue(interfaceType, out var generatedProxy))
                {
                    generatedProxy = GenerateProxyType(baseType, interfaceType);
                    interfaceToProxy[interfaceType] = generatedProxy;
                }

                return generatedProxy;
            }
        }

        /// <summary> 生成代理类型 </summary>
        /// <param name="baseType"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        private Type GenerateProxyType(Type baseType, Type interfaceType)
        {
            var baseTypeInfo = baseType.GetTypeInfo();
            if (!interfaceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"InterfaceType_Must_Be_Interface, {interfaceType.FullName}", nameof(interfaceType));
            }

            if (baseTypeInfo.IsSealed)
            {
                throw new ArgumentException($"BaseType_Cannot_Be_Sealed, {baseTypeInfo.FullName}", nameof(baseType));
            }

            if (baseTypeInfo.IsAbstract)
            {
                throw new ArgumentException($"BaseType_Cannot_Be_Abstract {baseType.FullName}", nameof(baseType));
            }

            var pb = _proxyAssembly.CreateProxy("SpearProxy", baseType);

            foreach (var t in interfaceType.GetTypeInfo().ImplementedInterfaces)
                pb.AddInterfaceImpl(t);

            pb.AddInterfaceImpl(interfaceType);

            var generatedProxyType = pb.CreateType();
            return generatedProxyType;
        }

        private ProxyMethodResolverContext Resolve(object[] args)
        {
            var packed = new PackedArgs(args);
            var method = _proxyAssembly.ResolveMethodToken(packed.DeclaringType, packed.MethodToken);
            if (method.IsGenericMethodDefinition)
                method = ((MethodInfo)method).MakeGenericMethod(packed.GenericTypes);

            return new ProxyMethodResolverContext(packed, method);
        }

        public object Invoke(object[] args)
        {
            var context = Resolve(args);

            // Call (protected method) DispatchProxyAsync.Invoke()
            object returnValue = null;
            try
            {
                Debug.Assert(_dispatchProxyInvokeMethod != null);
                returnValue = _dispatchProxyInvokeMethod.Invoke(context.Packed.DispatchProxy,
                    new object[] {context.Method, context.Packed.Args});
                context.Packed.ReturnValue = returnValue;
            }
            catch (TargetInvocationException tie)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            }
            catch (Exception ex)
            {
                
            }

            return returnValue;
        }

        public async Task InvokeAsync(object[] args)
        {
            var context = Resolve(args);

            // Call (protected Task method) NetCoreStackDispatchProxy.InvokeAsync()
            try
            {
                Debug.Assert(_dispatchProxyInvokeAsyncMethod != null);
                await (Task)_dispatchProxyInvokeAsyncMethod.Invoke(context.Packed.DispatchProxy,
                                                                       new object[] { context.Method, context.Packed.Args });
            }
            catch (TargetInvocationException tie)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<T> InvokeAsync<T>(object[] args)
        {
            var context = Resolve(args);

            var returnValue = default(T);
            try
            {
                Debug.Assert(_dispatchProxyInvokeAsyncTMethod != null);
                var genericmethod = _dispatchProxyInvokeAsyncTMethod.MakeGenericMethod(typeof(T));
                returnValue = await (Task<T>)genericmethod.Invoke(context.Packed.DispatchProxy,
                                                                       new object[] { context.Method, context.Packed.Args });
                context.Packed.ReturnValue = returnValue;
            }
            catch (TargetInvocationException tie)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
            }
            return returnValue;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
