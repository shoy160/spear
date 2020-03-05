using ProtoBuf;
using Spear.Core.Message.Models;

namespace Spear.Codec.ProtoBuffer.Models
{
    [ProtoContract]
    public class ProtoBufferResult : DMessageResult<ProtoBufferDynamic>
    {
        [ProtoMember(1)]
        public override string Id { get; set; }

        /// <summary> 状态码 </summary>
        [ProtoMember(2)]
        public override int Code { get; set; }

        /// <summary> 错误消息 </summary>
        [ProtoMember(3)]
        public override string Message { get; set; }

        /// <summary> 数据实体 </summary>
        [ProtoMember(4)]
        public override ProtoBufferDynamic Content { get; set; }

        public ProtoBufferResult() { }

        public ProtoBufferResult(MessageResult message) : base(message)
        {
        }
    }
}
