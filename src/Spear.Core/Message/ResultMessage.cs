using System.Net;

namespace Spear.Core.Message
{
    /// <summary> 调用结果消息 </summary>
    public class ResultMessage
    {
        /// <summary> 状态码 </summary>
        public int Code { get; set; } = 200;
        /// <summary> 错误消息 </summary>
        public string Message { get; set; }
        /// <summary> 数据实体 </summary>
        public object Data { get; set; }

        public ResultMessage() { }

        public ResultMessage(object data)
        {
            Data = data;
        }

        public ResultMessage(string message, int code = (int)HttpStatusCode.InternalServerError)
        {
            Message = message;
            Code = code;
        }
    }
}
