using System.Collections.Generic;

namespace Spear.Core.Message
{
    /// <summary> 调用消息 </summary>
    public class InvokeMessage
    {
        /// <summary> 服务Id </summary>
        public string ServiceId { get; set; }

        public string Token { get; set; }

        public bool DecodeJOject { get; set; }

        public string ServiceKey { get; set; }
        /// <summary> 服务参数 </summary>
        public IDictionary<string, object> Parameters { get; set; }

        public IDictionary<string, object> Attachments { get; set; }
    }
}
