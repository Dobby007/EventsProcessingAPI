using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsChart.Rendering
{
    readonly struct RenderingRequest
    {
        public readonly long Offset;
        public readonly SegmentSize SegmentSize;
        public readonly int Width;
        public readonly int Height;


        public bool Covers(long offset, in SegmentSize segmentSize, int width, int height)
        {
            if (!segmentSize.Equals(SegmentSize) || height != Height)
                return false;

            return offset >= Offset && Offset 
                + Width * SegmentSize.DisplayedValue >= offset + width * segmentSize.DisplayedValue;
        }

        public RenderingRequest(long offset, SegmentSize segmentSize, int width, int height)
        {
            Offset = offset;
            SegmentSize = segmentSize;
            Width = width;
            Height = height;
        }
    }
}
