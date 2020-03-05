using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Spear.Core.Message.Models;

namespace Spear.Codec.ProtoBuffer.Models
{
    [ProtoContract]
    public class ProtoBufferInvoke
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        /// <summary> 服务Id </summary>
        [ProtoMember(2)]
        public string ServiceId { get; set; }

        /// <summary> 是否是通知 </summary>
        [ProtoMember(3)]
        public bool IsNotice { get; set; }

        /// <summary> 服务参数 </summary>
        [ProtoMember(4)]
        public IDictionary<string, ProtoBufferDynamic> Parameters { get; set; }

        [ProtoMember(5)]
        public IDictionary<string, string> Headers { get; set; }

        public ProtoBufferInvoke() { }
        public ProtoBufferInvoke(InvokeMessage message)
        {
            Id = message.Id;
            ServiceId = message.ServiceId;
            IsNotice = message.IsNotice;
            if (message.Parameters != null)
            {
                Parameters =
                    message.Parameters.ToDictionary(k => k.Key, v => new ProtoBufferDynamic(v.Value.GetValue()));
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
