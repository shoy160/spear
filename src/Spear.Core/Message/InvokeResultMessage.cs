namespace Spear.Core.Message
{
    /// <summary> 调用结果消息 </summary>
    public class InvokeResultMessage
    {
        /// <summary> 状态码 </summary>
        public int Code { get; set; } = 200;
        /// <summary> 错误消息 </summary>
        public string Message { get; set; }
        /// <summary> 数据实体 </summary>
        public object Data { get; set; }
    }
}
