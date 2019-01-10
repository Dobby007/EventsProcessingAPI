using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Density
{
    internal readonly ref struct PartialDensitiesParameters
    {
        public readonly long SegmentSizeOffset;
        public readonly int PartiallyCalculatedSegments;

        public PartialDensitiesParameters(long segmentSizeOffset, int partiallyCalculatedSegments)
        {
            SegmentSizeOffset = segmentSizeOffset;
            PartiallyCalculatedSegments = partiallyCalculatedSegments;
        }
    }
}
