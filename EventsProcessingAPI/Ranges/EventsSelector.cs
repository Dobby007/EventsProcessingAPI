using EventsDomain;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsProcessingAPI.Ranges
{
    static class EventsSelector
    {
        public static Range GetRangeWithEvents(in ReadOnlySpan<Bucket> buckets, in RangeRequest rangeRequest, bool includeEventsOutOfRange = true)
        {
            var range = RangeSelector.GetRange(buckets, rangeRequest);
            if (range.IsFound)
                return range;

            if (includeEventsOutOfRange && range.IsNearestEventFound)
            {
                if (buckets[range.NearestBucketIndex].Events[range.NearestEventIndex].EventType == EventType.Start)
                    return new Range(range.NearestBucketIndex, range.NearestBucketIndex, range.NearestEventIndex, range.NearestEventIndex);
            }

            return range;
        }
    }
}
