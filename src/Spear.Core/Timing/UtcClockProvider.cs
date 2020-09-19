using System;

namespace Spear.Core.Timing
{
    public class UtcClockProvider : IClockProvider
    {
        public DateTime Now => DateTime.UtcNow;

        public DateTime Normalize(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dateTime.Kind == DateTimeKind.Local
                ? dateTime.ToUniversalTime()
                : dateTime;
        }
    }
}
