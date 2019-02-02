using Spear.Core.Micro.Services;
using System;

namespace Spear.Core
{
    /// <summary> 协议属性 </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolAttribute : Attribute
    {
        public ServiceProtocol Protocol { get; }

        public ProtocolAttribute(ServiceProtocol protocol)
        {
            Protocol = protocol;
        }
    }
}
