using System;
using Newtonsoft.Json;
using ProtoBuf;
using Spear.Core;

namespace Spear.Codec.ProtoBuffer.Models
{
    [ProtoContract]
    public class ProtoBufferDynamic
    {
        [ProtoMember(1)]
        public string ContentType { get; set; }

        [ProtoMember(2)]
        public byte[] Content { get; set; }

        public ProtoBufferDynamic() { }

        public ProtoBufferDynamic(object item)
        {
            if (item == null) return;
            var type = item.GetType();
            ContentType = type.TypeName();
            var code = Type.GetTypeCode(type);
            if (code == TypeCode.Object)
            {
                Content = JsonConvert.SerializeObject(item).Serialize();
            }
            else
            {
                Content = item.Serialize();
            }
        }

        public object GetValue()
        {
            if (Content == null || string.IsNullOrWhiteSpace(ContentType))
                return null;
            var type = Type.GetType(ContentType);
            var code = Type.GetTypeCode(type);
            if (code == TypeCode.Object)
            {
                var content = Content.Deserialize<string>();
                return JsonConvert.DeserializeObject(content, type);
            }

            return Content.Deserialize(type);
        }
    }
}
