using Spear.Core.Message.Models;

namespace Spear.Core.Message
{
    public static class ContentTypes
    {
        /// <summary> 调用消息类型 </summary>
        public static readonly string InvokeType = typeof(InvokeMessage).FullName;
        /// <summary> 调用结果消息类型 </summary>
        public static readonly string InvokeResultType = typeof(ResultMessage).FullName;
    }
}
