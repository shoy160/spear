using System.Reflection;
using System.Reflection.Emit;

namespace Spear.ProxyGenerator.Proxy
{
    internal sealed class EventAccessorInfo
    {
        public MethodInfo InterfaceAddMethod { get; }
        public MethodInfo InterfaceRemoveMethod { get; }
        public MethodInfo InterfaceRaiseMethod { get; }
        public MethodBuilder AddMethodBuilder { get; set; }
        public MethodBuilder RemoveMethodBuilder { get; set; }
        public MethodBuilder RaiseMethodBuilder { get; set; }

        public EventAccessorInfo(MethodInfo interfaceAddMethod, MethodInfo interfaceRemoveMethod, MethodInfo interfaceRaiseMethod)
        {
            InterfaceAddMethod = interfaceAddMethod;
            InterfaceRemoveMethod = interfaceRemoveMethod;
            InterfaceRaiseMethod = interfaceRaiseMethod;
        }
    }
}
