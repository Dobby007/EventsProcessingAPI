using EventsDomain;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using EventsProcessingAPI.Density;
using EventsProcessingAPI.Enumeration;
using EventsProcessingAPI.Common;

namespace EventsProcessingAPI
{
    /// <summary>
    /// Container used to store events, payloads and buckets
    /// </summary>
    public sealed class BucketContainer
    {
        /// <summary>
        /// Container used to store density hints
        /// </summary>
        internal DensityHintContainer DensityHintContainer { get; set; }

        /// <summary>
        /// All buckets that are stored in the current bucket container
        /// </summary>
        public Bucket[] Buckets { get; }

        /// <summary>
        /// Time when session was started
        /// </summary>
        public long FirstTimestamp { get; }

        /// <summary>
        /// Last event time in the current bucket container
        /// </summary>
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


        /// <summary>
        /// Returns segment sizes that are preferred to use when densities are calculated for the current bucket container.
        /// These segment sizes guaratee maximum perfomance gain.
        /// </summary>
        /// <param name="segmentsCount">Count of segments that need to be displayed</param>
        /// <returns>All the events that were found</returns>
        public SegmentSize[] GetPreferredSegmentSizes(ushort segmentsCount)
        {
            var lastBucket = GetLastBucket();

            if (lastBucket == null)
                return Array.Empty<SegmentSize>();

            var endTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            var startTime = FirstTimestamp;

            return SegmentSizeHelper.GetPreferredSegmentSizes(startTime, endTime, segmentsCount);
        }

        /// <summary>
        /// Returns segment sizes that are preferred to use when densities are calculated. These segment sizes guaratee maximum perfomance gain.
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (exclusive)</param>
        /// <param name="segmentsCount">Count of segments that need to be displayed</param>
        /// <returns>Preferred segment sizes</returns>
        public SegmentSize[] GetPreferredSegmentSizes(long start, long end, ushort segmentsCount)
        {
            return SegmentSizeHelper.GetPreferredSegmentSizes(start, end, segmentsCount);
        }

        /// <summary>
        /// Select first bucket in the current bucket container
        /// </summary>
        public Bucket GetFirstBucket()
        {
            return Buckets.Length > 0 ? Buckets[0] : null;
        }

        /// <summary>
        /// Select last bucket in the current bucket container
        /// </summary>
        public Bucket GetLastBucket()
        {
            return Buckets.Length > 0 ? Buckets[Buckets.Length - 1] : null;
        }

        /// <summary>
        /// Selects all events that are related to the given interval
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (exclusive)</param>
        /// <returns>All the events that were found</returns>
        public EventEnumerable GetEvents(long start, long end, bool includeEventsOutOfRange = true)
        {
            var range = EventsSelector.GetRangeWithEvents(Buckets, new RangeRequest(start, end), includeEventsOutOfRange);
            if (!range.IsFound)
                return EventEnumerable.Empty;

            return new EventEnumerable(
                new Memory<Bucket>(Buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }

        /// <summary>
        /// Selects all real events that are related to the given interval. Real event is the event with absolute time and the type.
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (exclusive)</param>
        /// <returns>All the events that were found</returns>
        public RealEventEnumerable GetRealEvents(long start, long end, bool includeEventsOutOfRange = true)
        {
            var range = EventsSelector.GetRangeWithEvents(Buckets, new RangeRequest(start, end), includeEventsOutOfRange);
            if (!range.IsFound)
                return RealEventEnumerable.Empty;

            return new RealEventEnumerable(
                new Memory<Bucket>(Buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }

        /// <summary>
        /// Selects all payloads that are related to the given interval
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (exclusive)</param>
        /// <returns>All the payloads that were found</returns>
        public PayloadEnumerable GetPayloads(long start, long end, bool includeEventsOutOfRange = true)
        {
            var range = EventsSelector.GetRangeWithEvents(Buckets, new RangeRequest(start, end), includeEventsOutOfRange);
            if (!range.IsFound)
                return PayloadEnumerable.Empty;

            return new PayloadEnumerable(
                new Memory<Bucket>(Buckets, range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );
        }

        /// <summary>
        /// Calculates densities for specific interval with specific segment size
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (inclusive)</param>
        /// <param name="segmentSize">Segment size</param>
        /// <returns>All the densities that were calculated for the given interval</returns>
        public double[] GetDensities(long start, long end, long segmentSize)
        {
            return DensityCalculationManager.GetDensities(this, start, end, segmentSize);
        }

        /// <summary>
        /// Calculates densities for specific interval with specific segment size using pre-allocated buffer
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (inclusive)</param>
        /// <param name="segmentSize">Segment size</param>
        /// <param name="targetBuffer">Pre-allocated buffer</param>
        /// <returns>Part of the pre-allocated buffer where calculated densities were set</returns>
        public Span<double> GetDensities(long start, long end, long segmentSize, ref double[] targetBuffer)
        {
            return DensityCalculationManager.GetDensities(this, start, end, segmentSize, ref targetBuffer);
        }

        /// <summary>
        /// Creates pre-allocated bufer
        /// </summary>
        /// <returns>Buffer to use for calling <see cref="GetDensities"/> method</returns>
        public double[] CreateBufferForDensities()
        {
            return new double[ushort.MaxValue];
        }

        /// <summary>
        /// Selects all buckets that are related to the given interval
        /// </summary>
        /// <param name="start">Start time (inclusive)</param>
        /// <param name="end">End time (exclusive)</param>
        /// <returns>All the buckets that were found</returns>
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
