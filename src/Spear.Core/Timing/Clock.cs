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
    }
}
