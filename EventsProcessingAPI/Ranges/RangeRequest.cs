using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Ranges
{
    public readonly struct RangeRequest
    {
        public readonly long Start;
        public readonly long End;
        public readonly long FirstTimeStamp;

        public RangeRequest(long start, long end, long firstTimeStamp)
        {
            Start = start;
            End = end;
            FirstTimeStamp = firstTimeStamp;
        }
    }
}
