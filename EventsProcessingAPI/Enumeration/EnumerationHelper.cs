using EventsDomain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Enumeration
{
    static class EnumerationHelper
    {

        public static bool MoveNext(in Span<Bucket> buckets, ref int bucketIndex, ref int eventIndex, int lastEventIndex)
        {
            int currentBucketIndex = bucketIndex;
            int currentEventIndex = eventIndex;

            if (currentBucketIndex == buckets.Length - 1 && currentEventIndex + 1 > lastEventIndex)
                return false;

            if (++currentEventIndex < buckets[currentBucketIndex].Events.Length)
            {
                eventIndex = currentEventIndex;
                return true;
            }

            if (++currentBucketIndex < buckets.Length)
            {
                bucketIndex = currentBucketIndex;
                eventIndex = 0;
                return true;
            }

            return false;
        }
    }
}
