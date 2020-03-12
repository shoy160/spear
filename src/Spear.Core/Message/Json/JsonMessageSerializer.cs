using System;
using System.Text;
using Newtonsoft.Json;
using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Message.Implementation;

namespace Spear.Core.Message.Json
{
    [Codec(ServiceCodec.Json)]
    public class JsonMessageSerializer : DMessageSerializer
    {
        public override byte[] Serialize(object value)
        {
            if (value == null) return new byte[0];

            var content = JsonConvert.SerializeObject(value);
            return Encoding.UTF8.GetBytes(content);
        }

        public override object Deserialize(byte[] data, Type type)
        {
            if (data == null) return null;
            var content = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(content, type);
        }


        public override T Deserialize<T>(byte[] data)
        {
            if (data == null) return default;
            var content = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
