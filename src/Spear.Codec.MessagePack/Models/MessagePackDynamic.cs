using System;
using MessagePack;
using Newtonsoft.Json;
using Spear.Core;

namespace Spear.Codec.MessagePack.Models
{
    [MessagePackObject]
    public class MessagePackDynamic
    {
        [Key(0)]
        public string ContentType { get; set; }

        [Key(1)]
        public byte[] Content { get; set; }

        public MessagePackDynamic() { }

        public MessagePackDynamic(object item)
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
