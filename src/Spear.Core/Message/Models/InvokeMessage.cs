using System.Collections.Generic;

namespace Spear.Core.Message.Models
{
    /// <summary> 调用消息 </summary>
    public class InvokeMessage : DMessage, IMessageInvoke
    {
        /// <summary> 服务Id </summary>
        public string ServiceId { get; set; }

        /// <summary> 是否是通知 </summary>
        public bool IsNotice { get; set; }

        /// <summary> 服务参数 </summary>
        public IDictionary<string, object> Parameters { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public InvokeMessage()
        {
            Parameters = new Dictionary<string, object>();
        }
    }
}
