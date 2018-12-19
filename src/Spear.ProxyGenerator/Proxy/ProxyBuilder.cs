using Spear.ProxyGenerator.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Spear.ProxyGenerator.Proxy
{
    internal class ProxyBuilder
    {
        private const int InvokeActionFieldAndCtorParameterIndex = 0;

        private static readonly MethodInfo SDelegateInvoke = typeof(ProxyHandler).GetMethod("InvokeHandle");
        private static readonly MethodInfo SDelegateInvokeAsync = typeof(ProxyHandler).GetMethod("InvokeAsyncHandle");
        private static readonly MethodInfo SDelegateinvokeAsyncT = typeof(ProxyHandler).GetMethod("InvokeAsyncHandleT");

        private readonly ProxyAssembly _assembly;
        private readonly TypeBuilder _tb;
        private readonly Type _proxyBaseType;
        private readonly List<FieldBuilder> _fields;

        internal ProxyBuilder(ProxyAssembly assembly, TypeBuilder tb, Type proxyBaseType)
        {
            _assembly = assembly;
            _tb = tb;
            _proxyBaseType = proxyBaseType;

            _fields = new List<FieldBuilder>
            {
                //tb.DefineField("_provider", typeof(IProxyProvider), FieldAttributes.Private),
                //tb.DefineField("_key", typeof(object), FieldAttributes.Private),
                tb.DefineField("_handler", typeof(ProxyHandler), FieldAttributes.Private),
            };
        }

        private static bool IsGenericTask(Type type)
        {
            var current = type;
            while (current != null)
            {
                if (current.GetTypeInfo().IsGenericType && current.GetGenericTypeDefinition() == typeof(Task<>))
                    return true;
                current = current.GetTypeInfo().BaseType;
            }
            return false;
        }

        private void Complete()
        {
            var args = new[]
            {
                typeof(IProxyProvider),
                typeof(object),
                typeof(ProxyHandler)
            };

            var cb = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);
            var il = cb.GetILGenerator();

            // chained ctor call
            var baseCtor = _proxyBaseType.GetTypeInfo().DeclaredConstructors
                .SingleOrDefault(c => c.IsPublic && c.GetParameters().Length == 2);
            Debug.Assert(baseCtor != null);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, baseCtor);

            // store all the fields
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg, 3);
            il.Emit(OpCodes.Stfld, _fields[0]);
            il.Emit(OpCodes.Ret);
        }

        internal Type CreateType()
        {
            Complete();
            return _tb.CreateTypeInfo().AsType();
        }

        internal void AddInterfaceImpl(Type iface)
        {
            // If necessary, generate an attribute to permit visibility
            // to internal types.
            _assembly.EnsureTypeIsVisible(iface);

            _tb.AddInterfaceImplementation(iface);

            // AccessorMethods -> Metadata mappings.
            var propertyMap = new Dictionary<MethodInfo, PropertyAccessorInfo>(MethodInfoEqualityComparer.Instance);
            foreach (var pi in iface.GetRuntimeProperties())
            {
                var ai = new PropertyAccessorInfo(pi.GetMethod, pi.SetMethod);
                if (pi.GetMethod != null)
                    propertyMap[pi.GetMethod] = ai;
                if (pi.SetMethod != null)
                    propertyMap[pi.SetMethod] = ai;
            }

            var eventMap = new Dictionary<MethodInfo, EventAccessorInfo>(MethodInfoEqualityComparer.Instance);
            foreach (var ei in iface.GetRuntimeEvents())
            {
                var ai = new EventAccessorInfo(ei.AddMethod, ei.RemoveMethod, ei.RaiseMethod);
                if (ei.AddMethod != null)
                    eventMap[ei.AddMethod] = ai;
                if (ei.RemoveMethod != null)
                    eventMap[ei.RemoveMethod] = ai;
                if (ei.RaiseMethod != null)
                    eventMap[ei.RaiseMethod] = ai;
            }

            foreach (var mi in iface.GetRuntimeMethods())
            {
                var mdb = AddMethodImpl(mi);
                if (propertyMap.TryGetValue(mi, out var associatedProperty))
                {
                    if (MethodInfoEqualityComparer.Instance.Equals(associatedProperty.InterfaceGetMethod, mi))
                        associatedProperty.GetMethodBuilder = mdb;
                    else
                        associatedProperty.SetMethodBuilder = mdb;
                }

                if (eventMap.TryGetValue(mi, out var associatedEvent))
                {
                    if (MethodInfoEqualityComparer.Instance.Equals(associatedEvent.InterfaceAddMethod, mi))
                        associatedEvent.AddMethodBuilder = mdb;
                    else if (MethodInfoEqualityComparer.Instance.Equals(associatedEvent.InterfaceRemoveMethod, mi))
                        associatedEvent.RemoveMethodBuilder = mdb;
                    else
                        associatedEvent.RaiseMethodBuilder = mdb;
                }
            }

            foreach (var pi in iface.GetRuntimeProperties())
            {
                var ai = propertyMap[pi.GetMethod ?? pi.SetMethod];
                var pb = _tb.DefineProperty(pi.Name, pi.Attributes, pi.PropertyType, pi.GetIndexParameters().Select(p => p.ParameterType).ToArray());
                if (ai.GetMethodBuilder != null)
                    pb.SetGetMethod(ai.GetMethodBuilder);
                if (ai.SetMethodBuilder != null)
                    pb.SetSetMethod(ai.SetMethodBuilder);
            }

            foreach (var ei in iface.GetRuntimeEvents())
            {
                var ai = eventMap[ei.AddMethod ?? ei.RemoveMethod];
                var eb = _tb.DefineEvent(ei.Name, ei.Attributes, ei.EventHandlerType);
                if (ai.AddMethodBuilder != null)
                    eb.SetAddOnMethod(ai.AddMethodBuilder);
                if (ai.RemoveMethodBuilder != null)
                    eb.SetRemoveOnMethod(ai.RemoveMethodBuilder);
                if (ai.RaiseMethodBuilder != null)
                    eb.SetRaiseMethod(ai.RaiseMethodBuilder);
            }
        }

        /// <summary> 添加方法实现 </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        private MethodBuilder AddMethodImpl(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            var paramTypes = ParamTypes(parameters, false);

            var mdb = _tb.DefineMethod(mi.Name, MethodAttributes.Public | MethodAttributes.Virtual, mi.ReturnType, paramTypes);
            if (mi.ContainsGenericParameters)
            {
                var ts = mi.GetGenericArguments();
                var ss = new string[ts.Length];
                for (var i = 0; i < ts.Length; i++)
                {
                    ss[i] = ts[i].Name;
                }
                var genericParameters = mdb.DefineGenericParameters(ss);
                for (var i = 0; i < genericParameters.Length; i++)
                {
                    genericParameters[i].SetGenericParameterAttributes(ts[i].GetTypeInfo().GenericParameterAttributes);
                }
            }
            var il = mdb.GetILGenerator();

            var args = new ParametersArray(il, paramTypes);

            // object[] args = new object[paramCount];
            il.Emit(OpCodes.Nop);
            var argsArr = new GenericArray<object>(il, ParamTypes(parameters, true).Length);

            for (var i = 0; i < parameters.Length; i++)
            {
                // args[i] = argi;
                if (parameters[i].IsOut) continue;
                argsArr.BeginSet(i);
                args.Get(i);
                argsArr.EndSet(parameters[i].ParameterType);
            }

            // object[] packed = new object[PackedArgs.PackedTypes.Length];
            var packedArr = new GenericArray<object>(il, PackedArgs.PackedTypes.Length);

            // packed[PackedArgs.DispatchProxyPosition] = this;
            packedArr.BeginSet(PackedArgs.DispatchProxyPosition);
            il.Emit(OpCodes.Ldarg_0);
            packedArr.EndSet(typeof(ProxyExecutor));

            // packed[PackedArgs.DeclaringTypePosition] = typeof(iface);
            var typeGetTypeFromHandle = typeof(Type).GetRuntimeMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) });
            _assembly.GetTokenForMethod(mi, out var declaringType, out var methodToken);
            packedArr.BeginSet(PackedArgs.DeclaringTypePosition);
            il.Emit(OpCodes.Ldtoken, declaringType);
            il.Emit(OpCodes.Call, typeGetTypeFromHandle);
            packedArr.EndSet(typeof(object));

            // packed[PackedArgs.MethodTokenPosition] = iface method token;
            packedArr.BeginSet(PackedArgs.MethodTokenPosition);
            il.Emit(OpCodes.Ldc_I4, methodToken);
            packedArr.EndSet(typeof(int));

            // packed[PackedArgs.ArgsPosition] = args;
            packedArr.BeginSet(PackedArgs.ArgsPosition);
            argsArr.Load();
            packedArr.EndSet(typeof(object[]));

            // packed[PackedArgs.GenericTypesPosition] = mi.GetGenericArguments();
            if (mi.ContainsGenericParameters)
            {
                packedArr.BeginSet(PackedArgs.GenericTypesPosition);
                var genericTypes = mi.GetGenericArguments();
                var typeArr = new GenericArray<Type>(il, genericTypes.Length);
                for (var i = 0; i < genericTypes.Length; ++i)
                {
                    typeArr.BeginSet(i);
                    il.Emit(OpCodes.Ldtoken, genericTypes[i]);
                    il.Emit(OpCodes.Call, typeGetTypeFromHandle);
                    typeArr.EndSet(typeof(Type));
                }
                typeArr.Load();
                packedArr.EndSet(typeof(Type[]));
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.IsByRef) continue;
                args.BeginSet(i);
                argsArr.Get(i);
                args.EndSet(i, typeof(object));
            }

            var invokeMethod = SDelegateInvoke;
            if (mi.ReturnType == typeof(Task))
            {
                invokeMethod = SDelegateInvokeAsync;
            }
            if (IsGenericTask(mi.ReturnType))
            {
                var returnTypes = mi.ReturnType.GetGenericArguments();
                invokeMethod = SDelegateinvokeAsyncT.MakeGenericMethod(returnTypes);
            }

            // Call AsyncDispatchProxyGenerator.Invoke(object[]), InvokeAsync or InvokeAsyncT
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _fields[InvokeActionFieldAndCtorParameterIndex]);
            packedArr.Load();
            il.Emit(OpCodes.Callvirt, invokeMethod);
            if (mi.ReturnType != typeof(void))
            {
                ProxyCodes.Convert(il, typeof(object), mi.ReturnType, false);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }

            il.Emit(OpCodes.Ret);

            _tb.DefineMethodOverride(mdb, mi);
            return mdb;
        }

        private static Type[] ParamTypes(IReadOnlyList<ParameterInfo> parms, bool noByRef)
        {
            var types = new Type[parms.Count];
            for (var i = 0; i < parms.Count; i++)
            {
                types[i] = parms[i].ParameterType;
                if (noByRef && types[i].IsByRef)
                    types[i] = types[i].GetElementType();
            }
            return types;
        }
    }
}
