using System;

namespace Spear.Core.Timing
{
    public static class Clock
    {
        private static IClockProvider _clockProvider;

        public static IClockProvider ClockProvider
        {
            get { return _clockProvider; }
            set
            {
                if (value == null) return;
                _clockProvider = value;
            }
        }

        static Clock()
        {
            _clockProvider = new LocalClockProvider();
        }

        public static DateTime Now => _clockProvider.Now;

        public static DateTime Normalize(DateTime dateTime)
        {
            return _clockProvider.Normalize(dateTime);
        }

        public static string ShowTime(this DateTime dateTime, string format = null)
        {
            var sp = Normalize(Now) - Normalize(dateTime);
            if (sp.TotalMinutes < 1) return "刚刚";
            if (sp.TotalHours < 1) return sp.Minutes + "分钟前";
            if (sp.TotalDays < 1) return sp.Hours + "小时前";
            return dateTime.ToString(format ?? "yyyy-MM-dd");
        }
    }
}
