using System;
using System.Collections.Generic;
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
                case TimeUnit.CpuTick:
                    return TimeUnitDurations.CpuTick;
                case TimeUnit.Microsecond:
                    return TimeUnitDurations.Microsecond;
                case TimeUnit.Millisecond:
                    return TimeUnitDurations.Millisecond;
                case TimeUnit.Second:
                    return TimeUnitDurations.Second;
                case TimeUnit.Minute:
                    return TimeUnitDurations.Minute;
                case TimeUnit.Hour:
                    return TimeUnitDurations.Hour;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTimeUnitDurationInMicroseconds(this TimeUnit timeUnit)
        {
            return GetTimeUnitDuration(timeUnit) / 10;
        }

        public static TimeUnit GetCeilingTimeUnit(long segmentSize)
        {
            if (segmentSize == 1)
                return TimeUnit.CpuTick;

            var timeUnit = TimeUnit.Microsecond;
            while (timeUnit.GetTimeUnitDuration() < segmentSize && timeUnit != TimeUnit.Hour)
            {
                timeUnit = (TimeUnit)((byte)timeUnit << 1);
            }

            return timeUnit;
        }

        public static TimeUnit GetFloorTimeUnit(long segmentSize)
        {
            if (segmentSize < 10)
                return TimeUnit.CpuTick;

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
                case TimeUnit.CpuTick:
                    return "tick";
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

        public static (double Value, TimeUnit Unit) ConvertTicksToTime(long ticks)
        {
            TimeUnit optimalTimeUnit = GetFloorTimeUnit(ticks);
            double time = ticks / (double)optimalTimeUnit.GetTimeUnitDuration();
            return (time, optimalTimeUnit);
        }

        public static bool TryGetNextTimeUnit(TimeUnit unit, out TimeUnit nextTimeUnit)
        {
            if (unit == TimeUnit.Hour)
            {
                nextTimeUnit = default;
                return false;
            }

            nextTimeUnit = (TimeUnit)((byte)unit << 1);
            return true;

        }

        public static IEnumerable<TimeUnit> GetAllTimeUnits()
        {
            TimeUnit current = TimeUnit.Microsecond;
            do
            {
                yield return current;
            }
            while ((current = (TimeUnit)((byte)current << 1)) <= TimeUnit.Hour);
        }
    }
}
