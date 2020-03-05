using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Spear.Core.Message.Models;

namespace Spear.Codec.MessagePack.Models
{
    [MessagePackObject]
    public class MessagePackInvoke
    {
        [Key(0)]
        public string Id { get; set; }

        /// <summary> 服务Id </summary>
        [Key(1)]
        public string ServiceId { get; set; }

        /// <summary> 是否是通知 </summary>
        [Key(2)]
        public bool IsNotice { get; set; }

        /// <summary> 服务参数 </summary>
        [Key(3)]
        public IDictionary<string, MessagePackDynamic> Parameters { get; set; }

        [Key(4)]
        public IDictionary<string, string> Headers { get; set; }

        public MessagePackInvoke() { }
        public MessagePackInvoke(InvokeMessage message)
        {
            Id = message.Id;
            ServiceId = message.ServiceId;
            IsNotice = message.IsNotice;
            if (message.Parameters != null)
            {
                Parameters =
                    message.Parameters.ToDictionary(k => k.Key, v => new MessagePackDynamic(v.Value.GetValue()));
            }

            if (message.Headers != null)
            {
                Headers = message.Headers.ToDictionary(k => k.Key, v => v.Value);
            }
        }

        public InvokeMessage GetValue()
        {
            var message = new InvokeMessage
            {
                Id = Id,
                ServiceId = ServiceId,
                IsNotice = IsNotice
            };
            if (Parameters != null)
            {
                message.Parameters = Parameters.ToDictionary(k => k.Key, v => new DynamicMessage(v.Value.GetValue()));
            }

            if (Headers != null)
            {
                message.Headers = Headers.ToDictionary(k => k.Key, v => v.Value);
            }
            return message;
        }
    }
}
