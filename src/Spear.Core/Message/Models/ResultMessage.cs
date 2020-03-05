using System.Net;

namespace Spear.Core.Message.Models
{
    /// <summary> 调用结果消息 </summary>
    public class MessageResult : DMessage, IMessageResult
    {
        /// <summary> 状态码 </summary>
        public int Code { get; set; } = 200;

        /// <summary> 错误消息 </summary>
        public string Message { get; set; }

        /// <summary> 数据实体 </summary>
        public object Content { get; set; }

        public MessageResult() { }

        public MessageResult(string message, int code = (int)HttpStatusCode.InternalServerError)
        {
            Message = message;
            Code = code;
        }
    }
}
