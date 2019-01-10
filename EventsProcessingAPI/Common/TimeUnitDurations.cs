using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Common
{
    public static class TimeUnitDurations
    {
        public const long CpuTick = 1L;
        public const long Microsecond = 10L;
        public const long Millisecond = 10000L;
        public const long Second = 10000L * 1000;
        public const long Minute = 10000L * 1000 * 60;
        public const long Hour = 10000L * 1000 * 60 * 60;
    }
}
