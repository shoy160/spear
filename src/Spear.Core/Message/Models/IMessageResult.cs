using Spear.Core.Message.Codec;

namespace Spear.Core.Message.Models
{
    public interface IMessageResult : IMessageResult<object> { }
    public interface IMessageResult<TDynamice> : IMessage
    {
        /// <summary> 状态码 </summary>
        int Code { get; set; }

        /// <summary> 错误消息 </summary>
        string Message { get; set; }

        /// <summary> 数据实体 </summary>
        TDynamice Content { get; set; }
    }
}
