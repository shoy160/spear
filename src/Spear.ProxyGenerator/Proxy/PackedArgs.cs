using Spear.ProxyGenerator.Impl;
using System;

namespace Spear.ProxyGenerator.Proxy
{
    internal class PackedArgs
    {
        internal const int DispatchProxyPosition = 0;
        internal const int DeclaringTypePosition = 1;
        internal const int MethodTokenPosition = 2;
        internal const int ArgsPosition = 3;
        internal const int GenericTypesPosition = 4;
        internal const int ReturnValuePosition = 5;

        internal static readonly Type[] PackedTypes =
            {typeof(object), typeof(Type), typeof(int), typeof(object[]), typeof(Type[]), typeof(object)};

        private readonly object[] _args;

        internal PackedArgs() : this(new object[PackedTypes.Length])
        {
        }

        internal PackedArgs(object[] args)
        {
            _args = args;
        }

        internal ProxyExecutor DispatchProxy => (ProxyExecutor)_args[DispatchProxyPosition];
        internal Type DeclaringType => (Type)_args[DeclaringTypePosition];
        internal int MethodToken => (int)_args[MethodTokenPosition];
        internal object[] Args => (object[])_args[ArgsPosition];
        internal Type[] GenericTypes => (Type[])_args[GenericTypesPosition];

        internal object ReturnValue
        {
            /*get { return args[ReturnValuePosition]; }*/
            set => _args[ReturnValuePosition] = value;
        }
    }
}
