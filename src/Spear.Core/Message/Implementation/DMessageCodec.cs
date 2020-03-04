using System;
using System.Threading.Tasks;

namespace Spear.Core.Message.Implementation
{
    public abstract class DMessageCodec : IMessageCodec
    {
        protected abstract byte[] OnEncode(object message);
        protected abstract object OnDecode(byte[] data, Type type);

        public async Task<byte[]> EncodeAsync(object message)
        {
            if (message == null) return new byte[0];
            var buffer = OnEncode(message);
            return await buffer.Zip();
        }

        public async Task<object> DecodeAsync(byte[] data, Type type)
        {
            var buffer = await data.UnZip();
            var obj = OnDecode(buffer, type);
            var result = obj.CastTo(type);
            return result;
        }
    }
}
