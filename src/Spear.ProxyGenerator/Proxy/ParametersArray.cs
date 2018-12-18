using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Spear.ProxyGenerator.Proxy
{
    internal class ParametersArray
    {
        private readonly ILGenerator _il;
        private readonly Type[] _paramTypes;
        internal ParametersArray(ILGenerator il, Type[] paramTypes)
        {
            _il = il;
            _paramTypes = paramTypes;
        }

        internal void Get(int i)
        {
            _il.Emit(OpCodes.Ldarg, i + 1);
        }

        internal void BeginSet(int i)
        {
            _il.Emit(OpCodes.Ldarg, i + 1);
        }

        internal void EndSet(int i, Type stackType)
        {
            Debug.Assert(_paramTypes[i].IsByRef);
            var argType = _paramTypes[i].GetElementType();
            ProxyCodes.Convert(_il, stackType, argType, false);
            ProxyCodes.Stind(_il, argType);
        }
    }
}
