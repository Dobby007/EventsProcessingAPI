using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Density
{
    internal readonly ref struct DensityCalculationRequest
    {
        public readonly long Start;
        public readonly long End;
        public readonly long SegmentSize;

        public DensityCalculationRequest(long start, long end, long segmentSize)
        {
            Start = start;
            End = end;
            SegmentSize = segmentSize;
        }
    }
}
