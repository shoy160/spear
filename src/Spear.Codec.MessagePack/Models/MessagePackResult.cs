using MessagePack;
using Spear.Core.Message.Models;

namespace Spear.Codec.MessagePack.Models
{
    [MessagePackObject]
    public class MessagePackResult : DMessageResult<MessagePackDynamic>
    {
        [Key(0)]
        public override string Id { get; set; }

        /// <summary> 状态码 </summary>
        [Key(1)]
        public override int Code { get; set; }

        /// <summary> 错误消息 </summary>
        [Key(2)]
        public override string Message { get; set; }

        /// <summary> 数据实体 </summary>
        [Key(3)]
        public override MessagePackDynamic Content { get; set; }

        public MessagePackResult() { }

        public MessagePackResult(MessageResult message) : base(message)
        {
        }
    }
}
