using System;
using Spear.Core.Message.Codec;

namespace Spear.Core.Message.Models
{
    public class DMessage : IMessage
    {
        public virtual string Id { get; set; }

        public DMessage(string id = null)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        }
    }
}
