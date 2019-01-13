using EventsDomain;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using EventsProcessingAPI.Density;
using EventsProcessingAPI.Enumeration;
using EventsProcessingAPI.Common;

namespace EventsProcessingAPI
{
    public sealed class BucketContainer
    {
        internal DensityHintContainer DensityHintContainer { get; set; }
        public Bucket[] Buckets { get; }
        public long FirstTimestamp { get; }
        public long LastTimestamp
        {
            get
            {
                var lastBucket = GetLastBucket();
                if (lastBucket == null)
                    return 0;

                return lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            }
        }

        internal BucketContainer(Bucket[] buckets, long firstTimestamp)
        {
            Buckets = buckets;
            FirstTimestamp = firstTimestamp;
        }

        

        public SegmentSize[] GetPreferredSegmentSizes(ushort segmentsCount)
        {
            var lastBucket = GetLastBucket();

            if (lastBucket == null)
                return Array.Empty<SegmentSize>();

            var endTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            var startTime = FirstTimestamp;

            return SegmentSizeHelper.GetPreferredSegmentSizes(startTime, endTime, segmentsCount);
        }

        public SegmentSize[] GetPreferredSegmentSizes(long start, long end, ushort segmentsCount)
        {
            return SegmentSizeHelper.GetPreferredSegmentSizes(start, end, segmentsCount);
        }

        public Bucket GetFirstBucket()
        {
            return Buckets.Length > 0 ? Buckets[0] : null;
        }

        public Bucket GetLastBucket()
        {
            return Buckets.Length > 0 ? Buckets[Buckets.Length - 1] : null;
        }

        public EventEnumerable GetEvents(long start, long end, bool includeEventsOutOfRange = true)
        {
            var range = EventsSelector.GetRangeWithEvents(Buckets, new RangeRequest(start, end, FirstTimestamp), includeEventsOutOfRange);
            if (!range.IsFound)
                return EventEnumerable.Empty;

            return new EventEnumerable(
                new Memory<Bucket>(Buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }

        public RealEventEnumerable GetRealEvents(long start, long end, bool includeEventsOutOfRange = true)
        {
            var range = EventsSelector.GetRangeWithEvents(Buckets, new RangeRequest(start, end, FirstTimestamp), includeEventsOutOfRange);
            if (!range.IsFound)
                return RealEventEnumerable.Empty;

            return new RealEventEnumerable(
                new Memory<Bucket>(Buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }
        
        public PayloadEnumerable GetPayloads(long start, long end, bool includeEventsOutOfRange = true)
        {
            var range = RangeSelector.GetRange(Buckets, start, end);
            if (!range.IsFound)
                throw new RangeNotFoundException();

            return new PayloadEnumerable(
                new Memory<Bucket>(Buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }

        public double[] GetDensities(long start, long end, long segmentSize)
        {
            return DensityCalculator.GetDensities(this, start, end, segmentSize);
        }

        public Bucket[] GetBuckets(long start, long end)
        {
            var range = RangeSelector.GetRange(Buckets, start, end);
            if (!range.IsFound)
                throw new RangeNotFoundException();
            
            var slice = new Bucket[range.Length];
            Array.Copy(Buckets, range.FirstBucketIndex, slice, 0, range.Length);
            return slice;
        }

    }
    
}
