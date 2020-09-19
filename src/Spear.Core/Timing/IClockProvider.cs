using System;

namespace Spear.Core.Timing
{
    public interface IClockProvider
    {
        DateTime Now { get; }
        DateTime Normalize(DateTime dateTime);
    }
}
