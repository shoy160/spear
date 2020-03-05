using System.Net;

namespace Spear.Core.Message.Models
{
    /// <summary> 调用结果消息 </summary>
    public class ResultMessage : DMessage
    {
        /// <summary> 状态码 </summary>
        public int Code { get; set; } = 200;

        /// <summary> 错误消息 </summary>
        public string Message { get; set; }

        /// <summary> 数据实体 </summary>
        public DynamicMessage Content { get; set; }

        public ResultMessage() { }

        public ResultMessage(string message, int code = (int)HttpStatusCode.InternalServerError)
        {
            Message = message;
            Code = code;
        }
    }
}
