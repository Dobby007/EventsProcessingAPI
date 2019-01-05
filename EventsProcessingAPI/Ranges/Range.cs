using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Ranges
{
    public readonly struct Range
    {
        public readonly int FirstBucketIndex;
        public readonly int LastBucketIndex;
        public readonly int FirstEventIndex;
        public readonly int LastEventIndex;

        public readonly int NearestBucketIndex;
        public readonly int NearestEventIndex;

        /// <summary>
        /// Length of range, i.e. count of buckets included into the range.
        /// </summary>
        public int Length => LastBucketIndex - FirstBucketIndex + 1;

        public readonly bool IsFound;

        public bool IsNearestEventFound => NearestBucketIndex >= 0 && NearestEventIndex >= 0;

        public Range(int firstBucketIndex, int lastBucketIndex, int firstEventIndex, int lastEventIndex)
        {
            FirstBucketIndex = firstBucketIndex;
            LastBucketIndex = lastBucketIndex;
            FirstEventIndex = firstEventIndex;
            LastEventIndex = lastEventIndex;
            NearestBucketIndex = -1;
            NearestEventIndex = -1;
            IsFound = true;
        }

        public Range(bool isFound) : this(-1, -1, -1, -1)
        {
            IsFound = isFound;
        }
        public Range(bool isFound, int nearestBucketIndex, int nearestEventIndex) : this(-1, -1, -1, -1)
        {
            NearestBucketIndex = nearestBucketIndex;
            NearestEventIndex = nearestEventIndex;
            IsFound = isFound;
        }

        public Range AddOffset(int offset)
        {
            return new Range(FirstBucketIndex + offset, LastBucketIndex + offset, FirstEventIndex, LastEventIndex);
        }
    }
}
