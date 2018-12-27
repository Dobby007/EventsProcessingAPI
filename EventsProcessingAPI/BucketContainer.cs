using EventsDomain;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C5;
using EventsProcessingAPI.Density;

namespace EventsProcessingAPI
{
    public sealed class BucketContainer
    {
        private readonly Bucket[] _buckets;
        /// <summary>
        /// This field is necessary because first timestamp and first event time may differ
        /// </summary>
        private readonly long _firstTimestamp;

        internal DensityHintContainer DensityHintContainer { get; set; }
        public Bucket[] Buckets => _buckets;
        
        public long FirstTimestamp => _firstTimestamp;

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
            _buckets = buckets;
            _firstTimestamp = firstTimestamp;
        }

        

        public long[] GetPreferredSegmentSizes(ushort segmentsCount)
        {
            var lastBucket = GetLastBucket();

            if (lastBucket == null)
                return Array.Empty<long>();

            var endTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            var startTime = FirstTimestamp;

            return DensityHelper.GetPreferredSegmentSizes(startTime, endTime, segmentsCount);
        }

        public long[] GetPreferredSegmentSizes(long start, long end, ushort segmentsCount)
        {
            return DensityHelper.GetPreferredSegmentSizes(start, end, segmentsCount);
        }

        public Bucket GetFirstBucket()
        {
            return _buckets.Length > 0 ? _buckets[0] : null;
        }

        public Bucket GetLastBucket()
        {
            return _buckets.Length > 0 ? _buckets[_buckets.Length - 1] : null;
        }

        public EventEnumerable GetEvents(long start, long end)
        {
            var range = RangeSelector.GetRange(this, start, end);
            if (!range.IsFound)
                throw new RangeNotFoundException();

            return new EventEnumerable(
                new Memory<Bucket>(_buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }

        public double[] GetDensities(long start, long end, long segmentSize)
        {
            return DensityCalculator.GetDensities(this, start, end, segmentSize);
        }

        public PayloadEnumerable GetPayloads(long start, long end)
        {
            var range = RangeSelector.GetRange(this, start, end);
            if (!range.IsFound)
                throw new RangeNotFoundException();

            return new PayloadEnumerable(
                new Memory<Bucket>(_buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }


        public Bucket[] GetBuckets(long start, long end)
        {
            var range = RangeSelector.GetRange(this, start, end);
            if (!range.IsFound)
                throw new RangeNotFoundException();
            
            var slice = new Bucket[range.Length];
            Array.Copy(_buckets, range.FirstBucketIndex, slice, 0, range.Length);
            return slice;
        }

    }
    
}
