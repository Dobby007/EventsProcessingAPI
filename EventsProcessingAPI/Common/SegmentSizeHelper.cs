using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Density
{
    internal static class SegmentSizeHelper
    {
        public static SegmentSize[] GetPreferredSegmentSizes(long startTime, long endTime, ushort segmentsCount)
        {
            if (endTime < startTime)
                throw new ArgumentException(nameof(endTime));

            var list = new List<SegmentSize>();
            long maxSegmentSize = (long)Math.Ceiling((endTime - startTime) / (double)segmentsCount);
            TimeUnit currentTimeUnit = TimeUnit.Microsecond;
            long segmentSize = currentTimeUnit.GetTimeUnitDuration();
            long previousSegmentSize = segmentSize;

            list.Add(new SegmentSize(1));
            while (segmentSize < maxSegmentSize)
            {
                previousSegmentSize = segmentSize;
                list.Add(new SegmentSize(segmentSize));
                switch (currentTimeUnit)
                {
                    case TimeUnit.Second:
                    case TimeUnit.Minute:
                        if (segmentSize % (20 * currentTimeUnit.GetTimeUnitDuration()) == 0)
                            segmentSize = segmentSize / 20 * 30;
                        else if (GetFirstDigit(segmentSize) == 4)
                            segmentSize = segmentSize / 4 * 10;
                        else
                            segmentSize *= 2;
                        break;
                    default:
                        if (GetFirstDigit(segmentSize) == 4)
                            segmentSize = segmentSize / 4 * 10;
                        else
                            segmentSize *= 2;
                        break;
                }
                

                TimeUnit nextTimeUnit = (TimeUnit)((byte)currentTimeUnit << 1);
                if (currentTimeUnit < TimeUnit.Hour && segmentSize >= nextTimeUnit.GetTimeUnitDuration())
                {
                    currentTimeUnit = nextTimeUnit;
                }
            }

            if (segmentSize > maxSegmentSize)
            {
                list.Add(new SegmentSize(previousSegmentSize, maxSegmentSize));
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
