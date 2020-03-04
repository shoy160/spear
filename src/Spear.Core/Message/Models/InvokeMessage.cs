using System.Collections.Generic;
using System.Linq;

namespace Spear.Core.Message.Models
{
    /// <summary> 调用消息 </summary>
    public class InvokeMessage : DMessage
    {
        /// <summary> 服务Id </summary>
        public string ServiceId { get; set; }

        /// <summary> 是否是通知 </summary>
        public bool IsNotice { get; set; }

        /// <summary> 服务参数 </summary>
        public IDictionary<string, DynamicObject> Parameters { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public InvokeMessage()
        {
            Parameters = new Dictionary<string, DynamicObject>();
        }

        public IDictionary<string, object> GetParameters()
        {
            return Parameters.ToDictionary(item => item.Key, item => item.Value.Data);
        }

        public void SetParameters(IDictionary<string, object> parameters)
        {
            foreach (var item in parameters)
            {
                if (Parameters.ContainsKey(item.Key))
                    Parameters.Remove(item.Key);
                Parameters.Add(item.Key, new DynamicObject(item.Value));
            }
        }
    }
}
