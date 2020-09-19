using System;

namespace Spear.Core.Timing
{
    public class LocalClockProvider : IClockProvider
    {
        public DateTime Now => DateTime.Now;

        public DateTime Normalize(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            return dateTime.Kind == DateTimeKind.Utc
                ? dateTime.ToLocalTime()
                : dateTime;
        }
    }
}
