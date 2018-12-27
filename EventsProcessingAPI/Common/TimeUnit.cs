using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Common
{
    public enum TimeUnit : byte
    {
        Microsecond = 1,
        Millisecond = 2,
        Second = 4,
        Minute = 8,
        Hour = 16
    }
}
