using System.Collections.Generic;
using Spear.Core.Message.Codec;

namespace Spear.Core.Message.Models
{
    public interface IMessageInvoke : IMessageInvoke<object> { }

    public interface IMessageInvoke<TDynamic> : IMessage
    {
        /// <summary> 服务Id </summary>
        string ServiceId { get; set; }

        /// <summary> 是否是通知 </summary>
        bool IsNotice { get; set; }

        /// <summary> 服务参数 </summary>
        IDictionary<string, TDynamic> Parameters { get; set; }

        IDictionary<string, string> Headers { get; set; }
    }
}
