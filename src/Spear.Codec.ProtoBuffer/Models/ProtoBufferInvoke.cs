using System.Collections.Generic;
using ProtoBuf;
using Spear.Core.Message.Models;

namespace Spear.Codec.ProtoBuffer.Models
{
    [ProtoContract]
    public class ProtoBufferInvoke : DMessageInvoke<ProtoBufferDynamic>
    {
        [ProtoMember(1)]
        public override string Id { get; set; }

        /// <summary> 服务Id </summary>
        [ProtoMember(2)]
        public override string ServiceId { get; set; }

        /// <summary> 服务参数 </summary>
        [ProtoMember(3)]
        public override IDictionary<string, ProtoBufferDynamic> Parameters { get; set; }

        [ProtoMember(4)]
        public override IDictionary<string, string> Headers { get; set; }

        public ProtoBufferInvoke() { }

        public ProtoBufferInvoke(InvokeMessage message) : base(message)
        {
        }
    }
}
