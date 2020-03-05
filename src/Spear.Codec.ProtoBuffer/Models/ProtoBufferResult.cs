using ProtoBuf;
using Spear.Core.Message.Models;

namespace Spear.Codec.ProtoBuffer.Models
{
    [ProtoContract]
    public class ProtoBufferResult
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        /// <summary> 状态码 </summary>
        [ProtoMember(2)]
        public int Code { get; set; }

        /// <summary> 错误消息 </summary>
        [ProtoMember(3)]
        public string Message { get; set; }

        /// <summary> 数据实体 </summary>
        [ProtoMember(4)]
        public ProtoBufferDynamic Content { get; set; }

        public ProtoBufferResult() { }

        public ProtoBufferResult(ResultMessage message)
        {
            Id = message.Id;
            Code = message.Code;
            Message = message.Message;
            if (message.Content != null)
            {
                Content = new ProtoBufferDynamic(message.Content.GetValue());
            }
        }

        public ResultMessage GetValue()
        {
            var result = new ResultMessage
            {
                Id = Id,
                Code = Code,
                Message = Message,
            };
            if (Content != null)
            {
                result.Content = new DynamicMessage(Content.GetValue());
            }

            return result;
        }
    }
}
