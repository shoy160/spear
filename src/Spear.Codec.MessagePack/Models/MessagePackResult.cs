using MessagePack;
using Spear.Core.Message.Models;

namespace Spear.Codec.MessagePack.Models
{
    [MessagePackObject]
    public class MessagePackResult
    {
        [Key(0)]
        public string Id { get; set; }

        /// <summary> 状态码 </summary>
        [Key(1)]
        public int Code { get; set; }

        /// <summary> 错误消息 </summary>
        [Key(2)]
        public string Message { get; set; }

        /// <summary> 数据实体 </summary>
        [Key(3)]
        public MessagePackDynamic Content { get; set; }

        public MessagePackResult() { }

        public MessagePackResult(ResultMessage message)
        {
            Id = message.Id;
            Code = message.Code;
            Message = message.Message;
            if (message.Content != null)
            {
                Content = new MessagePackDynamic(message.Content.GetValue());
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
