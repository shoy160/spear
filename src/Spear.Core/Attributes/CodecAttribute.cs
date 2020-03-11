using System;
using Spear.Core.Config;

namespace Spear.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CodecAttribute : Attribute
    {
        public ServiceCodec Codec { get; }

        public CodecAttribute(ServiceCodec codec)
        {
            Codec = codec;
        }
    }
}
