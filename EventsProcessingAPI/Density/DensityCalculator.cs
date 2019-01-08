using EventsDomain;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EventsProcessingAPI.Density
{
    internal static class DensityCalculator
    {
        /// <summary>
        /// Gets densities for segments with size equal to <paramref name="segmentSize"/>
        /// </summary>
        /// <param name="container">Bucket container</param>
        /// <param name="start">Start time</param>
        /// <param name="end">End time</param>
        /// <param name="segmentSize">Length/duration of one segment</param>
        /// <returns></returns>
        public static double[] GetDensities(BucketContainer container, long start, long end, long segmentSize)
        {
            if (end <= start)
            {
                throw new ArgumentException("Wrong interval. End timestamp must be greater than start timestamp.");
            }

            if (end - start < segmentSize)
            {
                throw new ArgumentException("Segment size is too big for this time interval", nameof(segmentSize));
            }


            var lastBucket = container.GetLastBucket();
            var maxTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            if (end > maxTime + 1)
            {
                end = maxTime + 1;
            }

            // Start time is out of range
            if (end < start)
                return Array.Empty<double>();

            ushort totalSegments = 0;
            try
            {
                totalSegments = checked((ushort)Math.Ceiling(Math.Max(end - start - 1, 1) / (double)segmentSize));
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Too small segment size for such a big range", nameof(segmentSize));
            }

            var densities = new double[totalSegments];
            int skippedDensities = -1;
            try
            {
                checked
                {
                    container.DensityHintContainer?.TrySetDensitiesUsingHints(
                        start - container.FirstTimestamp,
                        end - container.FirstTimestamp,
                        segmentSize,
                        densities,
                        out skippedDensities
                    );
                }
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException("Too big range of events");
            }

#if DEBUG
            if (skippedDensities == 0 && segmentSize >= 10_000_000)
                Debug.Fail("Density calculation is perfoming in unoptimized way. It can be eliminated by adjusting range and segment size to appropriate values.");
#endif

            if (skippedDensities < 0)
                skippedDensities = 0;

            if (skippedDensities == densities.Length)
                return densities;
            
            start += skippedDensities * segmentSize;
            

            var leftDensities = densities.AsMemory(skippedDensities);
            var span = leftDensities.Span;


            CalculateDensities(
                container.Buckets,
                start,
                start + leftDensities.Length * segmentSize,
                segmentSize,
                span,
                true,
                out long processedRange
            );

            return densities;
        }

        /// <summary>
        /// Gets densities for segments with size equal to <paramref name="segmentSize"/>
        /// </summary>
        /// <param name="buckets">Array of buckets</param>
        /// <param name="start">Start timestamp of the event range we want to find. If null, then it is a first event time in the first bucket of the buckets array.</param>
        /// <param name="segmentSize">Length/duration of one segment</param>
        /// <param name="finalize">Flag indicating that density should be calculated for the last incomplete segment if there is one</param>
        /// <param name="nextBatchStartTime">Next start time</param>
        /// <returns></returns>
        public static double[] GetDensities(ArraySegment<Bucket> bucketsArray, long? start, long segmentSize, bool finalize, out long nextBatchStartTime)
        {
            var buckets = bucketsArray.AsSpan();
            if (buckets.Length < 1)
            {
                throw new ArgumentException("Empty array of buckets is not allowed");
            }

            if (!start.HasValue)
                start = buckets[0].GetAbsoluteTimeForEvent(buckets[0].GetFirstEvent());

            long end = buckets[buckets.Length - 1].GetAbsoluteTimeForEvent(buckets[buckets.Length - 1].GetLastEvent()) + 1;

            ushort totalSegments = 0;
            try
            {
                if (finalize)
                    totalSegments = checked((ushort)Math.Ceiling((end - start.Value - 1) / (double)segmentSize));
                else
                    totalSegments = checked((ushort)Math.Floor((end - start.Value - 1) / (double)segmentSize));
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Too small segment size for such a big range", nameof(segmentSize));
            }

            var densitiesBuf = new double[totalSegments];
            CalculateDensities(
                bucketsArray.AsMemory(),
                start.Value,
                end,
                segmentSize,
                densitiesBuf,
                finalize,
                out long processedRange
            );

            nextBatchStartTime = start.Value + processedRange;
            
            return densitiesBuf;
        }


        private static void CalculateDensities(
            Memory<Bucket> bucketsArray,
            long start,
            long end,
            long segmentSize,
            Span<double> targetBuffer,
            bool finalize,
            out long processedRange
        )
        {
            var buckets = bucketsArray.Span;
            var range = RangeSelector.GetRange(bucketsArray.Span, start, end);
            if (!range.IsFound && !range.IsNearestEventFound)
                throw new RangeNotFoundException();

            EventEnumerable events;
            long filled = 0, unfilled = 0;
            long distance = 0;
            long lastEventTime = -1, currentEventTime = 0;
            int segmentIndex = 0;
            int eventsCount = 0;
            EventType lastEventType = EventType.Stop;
            Bucket currentBucket;
            EventBucketInfo firstEvent = default,
                        lastEvent = default,
                        lastAccountedEvent = default;

            // if there is no events in the range we need to find previous event type to calculate densities based on it
            if (!range.IsFound)
            {
                Event nearestEvent = buckets[range.NearestBucketIndex].Events[range.NearestEventIndex];
                lastEventType = nearestEvent.EventType;
                events = EventEnumerable.Empty;
            }
            else
            {
                buckets = buckets.Slice(range.FirstBucketIndex, range.Length);
                events = new EventEnumerable(
                    bucketsArray.Slice(range.FirstBucketIndex, range.Length),
                    range.FirstEventIndex,
                    range.LastEventIndex
                );
            }

            foreach (var ev in events)
            {
                currentBucket = buckets[ev.BucketIndex];
                if (eventsCount == 0)
                {
                    firstEvent = ev;
                    
                    // The first event in the range may not mean the start time by itself
                    // We can consider event time passed through `start` parameter as the first time
                    lastEventTime = start;
                    lastEventType = (EventType)((byte)~firstEvent.Event.EventType & 1);
                }
                

                if (lastEventTime >= 0)
                {
                    // Repeating events must be filtered in the IEventReader
                    if (lastEventType == ev.Event.EventType)
                        throw new InvalidOperationException("Repeating events are not acceptable");

                    currentEventTime = currentBucket.GetAbsoluteTimeForEvent(ev.Event);
                    // Current event time must be greater than the previous event time
                    if (currentEventTime < lastEventTime)
                        throw new ArithmeticException("Ëvents are not sorted in ascending order");

                    distance = filled + unfilled + (currentEventTime - lastEventTime);
#if DEBUG
                    var originalLastEventTime = lastEventTime;
#endif
                    int calculatedDensities = 0;
                    while (distance >= segmentSize)
                    {
                        long oldDistance = distance;
                        targetBuffer[segmentIndex++] = CalculateDensity(ref filled, ref unfilled, lastEventType, ref distance, segmentSize);
                        lastEventTime += oldDistance - distance;
                        distance = currentEventTime - lastEventTime;
                        calculatedDensities++;
                    }
                    if (calculatedDensities > 0)
                        lastAccountedEvent = distance > 0 ? lastEvent : ev;

                    switch (lastEventType)
                    {
                        case EventType.Start:
                            filled += currentEventTime - lastEventTime;
                            break;
                        case EventType.Stop:
                            unfilled += currentEventTime - lastEventTime;
                            break;
                    }

                    lastEventTime = currentEventTime;
                }
                else
                {
                    lastEventTime = currentBucket.GetAbsoluteTimeForEvent(ev.Event);
                }

                lastEventType = ev.Event.EventType;
                eventsCount++;
                lastEvent = ev;
            }

            
            // we need to check whether there are any more events to wait for or we have processed all events
            if (finalize && segmentIndex < targetBuffer.Length)
            {
                // we processed the whole range but some density values remain uncalculated
                distance = segmentSize;
                targetBuffer[segmentIndex++] = CalculateDensity(ref filled, ref unfilled, lastEventType, ref distance, segmentSize);

                // we need to set densities for segments that we didn't processed due to the lack of the events
                if (lastEventType == EventType.Start)
                    for (; segmentIndex < targetBuffer.Length; segmentIndex++)
                        targetBuffer[segmentIndex] = 1;

                processedRange = end - start;
            }
            else if (!finalize)
            {
                // there are some other events to wait for
                if (segmentIndex == 0)
                    processedRange = 0; // we have not calculated any densities
                else 
                    processedRange = segmentIndex * segmentSize;  // we have calculated some densities
            }
            else if (filled + unfilled == 0)
            {
                // we processed the whole range and calculated all the densities
                processedRange = end - start;
            }
            else 
            {
                // just for compiler to calm down - this isn't gonna happen (I hope so)
                throw new IndexOutOfRangeException("Density array is too short for this range");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalculateDensity(
            ref long filled, 
            ref long unfilled, 
            EventType previousEventType, 
            ref long distance, 
            long segmentSize)
        {
            if (filled + unfilled > segmentSize)
                throw new InvalidOperationException();

            if (distance < segmentSize)
                throw new InvalidOperationException();

            long usedAmount = segmentSize - (filled + unfilled);

            // normalizing
            switch (previousEventType)
            {
                case EventType.Start:
                    filled += usedAmount;
                    break;
                case EventType.Stop:
                    unfilled += usedAmount;
                    break;
            }

            // calculating
            double result = filled / (double)(filled + unfilled);

            // setting new state
            distance -= usedAmount;
            filled = 0;
            unfilled = 0;
            
            return result;
        }
        
    }
}
