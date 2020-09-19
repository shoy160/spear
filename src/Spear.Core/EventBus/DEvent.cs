using System;

namespace Spear.Core.EventBus
{
    public class DEvent
    {
        public DEvent()
        {
            EventId = Guid.NewGuid();
            EventTime = DateTime.Now;
        }

        public Guid EventId { get; }
        public DateTime EventTime { get; }
    }
}
