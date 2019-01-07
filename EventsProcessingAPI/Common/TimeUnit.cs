using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Common
{
    public enum TimeUnit : byte
    {
        CpuTick = 1,
        Microsecond = 2,
        Millisecond = 4,
        Second = 8,
        Minute = 16,
        Hour = 32
    }
}
