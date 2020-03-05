using System;

namespace Spear.Core.Message.Implementation
{
    public abstract class DMessageSerializer : IMessageSerializer
    {
        public abstract byte[] Serialize(object value);

        public virtual byte[] SerializeNoType(object value)
        {
            return Serialize(value);
        }

        public abstract object Deserialize(byte[] data, Type type);

        public virtual object DeserializeNoType(byte[] data, Type type)
        {
            return Deserialize(data, type);
        }

        public abstract T Deserialize<T>(byte[] data);
    }
}
