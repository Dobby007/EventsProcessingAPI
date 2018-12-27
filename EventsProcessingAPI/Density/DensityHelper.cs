using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Density
{
    internal static class DensityHelper
    {
        public static long[] GetPreferredSegmentSizes(long startTime, long endTime, ushort segmentsCount)
        {
            if (endTime < startTime)
                throw new ArgumentException(nameof(endTime));

            var list = new List<long>();
            long maxSegmentSize = (long)Math.Ceiling((endTime - startTime) / (double)segmentsCount);
            long segmentSize = 1;
            TimeUnit currentTimeUnit = TimeUnit.Microsecond;

            while (segmentSize <= maxSegmentSize)
            {
                list.Add(segmentSize);
                if (GetFirstDigit(segmentSize) == 4)
                    segmentSize = segmentSize / 4 * 10;
                else
                    segmentSize *= 2;

                TimeUnit nextTimeUnit = (TimeUnit)((byte)currentTimeUnit << 1);
                if (segmentSize >= nextTimeUnit.GetTimeUnitDuration() && currentTimeUnit < TimeUnit.Hour)
                {
                    currentTimeUnit = nextTimeUnit;
                }
            }

            if (segmentSize > maxSegmentSize)
            {
                if (currentTimeUnit == TimeUnit.Millisecond)
                    list.Add(maxSegmentSize);
                else
                    list.Add(segmentSize);
            }
            
            return list.ToArray();
        }

        private static byte GetFirstDigit(long number)
        {
            double current = number;
            while ((current = current / 10D) > 1);
            return (byte)(current * 10);

        }
    }
}
