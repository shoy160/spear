using System.Collections.Generic;
using MessagePack;
using Spear.Core.Message.Models;

namespace Spear.Codec.MessagePack.Models
{
    [MessagePackObject]
    public class MessagePackInvoke : DMessageInvoke<MessagePackDynamic>
    {
        [Key(0)]
        public override string Id { get; set; }

        /// <summary> 服务Id </summary>
        [Key(1)]
        public override string ServiceId { get; set; }

        /// <summary> 服务参数 </summary>
        [Key(2)]
        public override IDictionary<string, MessagePackDynamic> Parameters { get; set; }

        [Key(3)]
        public override IDictionary<string, string> Headers { get; set; }

        public MessagePackInvoke() { }
        public MessagePackInvoke(InvokeMessage message) : base(message)
        {
        }
    }
}
