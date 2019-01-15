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

        /// <summary>
        /// Creates request for searching a range of events
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">Start time (exclusive)</param>
        /// <param name="firstTimeStamp">First timestamp in buckets array</param>
        public RangeRequest(long start, long end, long firstTimeStamp)
        {
            Start = start;
            End = end;
            FirstTimeStamp = firstTimeStamp;
        }

        /// <summary>
        /// Creates request for searching a range of events
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">Start time (exclusive)</param>
        public RangeRequest(long start, long end)
        {
            Start = start;
            End = end;
            FirstTimeStamp = 0;
        }
    }
}
