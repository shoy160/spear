using System;

namespace Spear.Core.Message.Models
{
    public class DMessage
    {
        public string Id { get; set; }

        public DMessage(string id = null)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        }
    }
}
