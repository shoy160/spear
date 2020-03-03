using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Spear.Core.Message.Implementation
{
    public abstract class DMessageCodec : IMessageCodec
    {
        protected abstract byte[] EncodeInternal(object message);
        protected abstract object DecodeInternal(byte[] data, Type type);

        public async Task<byte[]> EncodeAsync(object message)
        {
            var buffer = EncodeInternal(message);
            return await buffer.Zip();
        }

        public async Task<object> DecodeAsync(byte[] data, Type type)
        {
            var buffer = await data.UnZip();
            var obj = DecodeInternal(buffer, type);
            var result = obj.CastTo(type);

            if (!(result is MicroMessage message))
                return result;

            if (message.IsInvoke)
            {
                message.Content = JsonConvert.DeserializeObject<InvokeMessage>(message.Content.ToString());
            }
            else if (message.IsResult)
            {
                message.Content = JsonConvert.DeserializeObject<ResultMessage>(message.Content.ToString());
            }

            return message;
        }
    }
}
