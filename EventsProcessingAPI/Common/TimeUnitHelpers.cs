using System;
using System.Runtime.CompilerServices;

namespace EventsProcessingAPI.Common
{
    public static class TimeUnitHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTimeUnitDuration(this TimeUnit timeUnit)
        {
            switch (timeUnit)
            {
                case TimeUnit.Microsecond:
                    return 10L;
                case TimeUnit.Millisecond:
                    return 10000L;
                case TimeUnit.Second:
                    return 10000L * 1000;
                case TimeUnit.Minute:
                    return 10000L * 1000 * 60;
                case TimeUnit.Hour:
                    return 10000L * 1000 * 60 * 60;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit));
            }
        }

        public static TimeUnit GetCeilingTimeUnit(long segmentSize)
        {
            var timeUnit = TimeUnit.Microsecond;
            while (timeUnit.GetTimeUnitDuration() < segmentSize && timeUnit != TimeUnit.Hour)
            {
                timeUnit = (TimeUnit)((byte)timeUnit << 1);
            }

            return timeUnit;
        }

        public static TimeUnit GetFloorTimeUnit(long segmentSize)
        {
            var floatSegmentSize = (double)segmentSize;
            var timeUnit = TimeUnit.Microsecond;
            while (floatSegmentSize / timeUnit.GetTimeUnitDuration() > 1 && timeUnit != TimeUnit.Hour)
            {
                timeUnit = (TimeUnit)((byte)timeUnit << 1);
            }

            if (floatSegmentSize / timeUnit.GetTimeUnitDuration() < 1)
                timeUnit = (TimeUnit)((byte)timeUnit >> 1);

            return timeUnit;
        }

        public static string GetTimeUnitAsString(this TimeUnit timeUnit)
        {
            switch (timeUnit)
            {
                case TimeUnit.Microsecond:
                    return "μs";
                case TimeUnit.Millisecond:
                    return "ms";
                case TimeUnit.Second:
                    return "s";
                case TimeUnit.Minute:
                    return "m";
                case TimeUnit.Hour:
                    return "h";
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit));
            }
        }
    }
}
