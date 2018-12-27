﻿using EventsDomain;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Range = EventsProcessingAPI.Ranges.RangeSelector.Range;

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
            if (end < start)
            {
                throw new ArgumentException("Wrong interval. End timestamp must be not less than start timestamp.");
            }


            var lastBucket = container.GetLastBucket();
            var maxTime = lastBucket.GetAbsoluteTimeForEvent(lastBucket.GetLastEvent());
            if (end > maxTime)
            {
                end = maxTime;
            }

            ushort totalSegments = 0;
            try
            {
                totalSegments = checked((ushort)Math.Ceiling((end - start) / (double)segmentSize));
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
                throw new InvalidOperationException("Too big range of events. I thought that it's never gonna happen.");
            }

#if DEBUG
            if (skippedDensities == 0 && segmentSize >= 1_000_000)
                Debug.Fail("Density calculation is perfoming in unoptimized way. It can be eliminated by adjusting range and segment size to appropriate values.");
#endif

            if (skippedDensities < 0)
                skippedDensities = 0;

            if (skippedDensities == densities.Length)
                return densities;
            
            start += skippedDensities * segmentSize;

            var events = container.GetEvents(start, end);
            CalculateDensities(
                events, 
                events.Buckets,
                start, 
                segmentSize, 
                densities.AsSpan(skippedDensities),
                true,
                out Range processedRange
            );

            return densities;
        }

        /// <summary>
        /// Gets densities for segments with size equal to <paramref name="segmentSize"/>
        /// </summary>
        /// <param name="buckets">Array of buckets</param>
        /// <param name="segmentSize">Length/duration of one segment</param>
        /// <param name="finalize">Flag indicating that density calculation must be done for incomplete segments</param>
        /// <param name="processedRange">Processed range</param>
        /// <returns></returns>
        public static double[] GetDensities(ArraySegment<Bucket> bucketsArray, long? start, long segmentSize, bool finalize, out Range processedRange)
        {
            var buckets = bucketsArray.AsSpan();
            if (buckets.Length < 1)
            {
                throw new ArgumentException("Empty array of buckets is not allowed");
            }

            if (!start.HasValue)
                start = buckets[0].GetAbsoluteTimeForEvent(buckets[0].GetFirstEvent());
            long end = buckets[buckets.Length - 1].GetAbsoluteTimeForEvent(buckets[buckets.Length - 1].GetLastEvent());

            ushort totalSegments = 0;
            try
            {
                totalSegments = checked((ushort)Math.Ceiling((end - start.Value) / (double)segmentSize));
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Too small segment size for such a big range", nameof(segmentSize));
            }
            
            var range = RangeSelector.GetRange(buckets, start.Value, end, start.Value);
            if (!range.IsFound)
                throw new RangeNotFoundException();

            var events = new EventEnumerable(
                bucketsArray.AsMemory(range.FirstBucketIndex, range.Length),
                range.FirstEventIndex,
                range.LastEventIndex
            );

            var densitiesBuf = new double[totalSegments];
            CalculateDensities(
                events,
                buckets.Slice(range.FirstBucketIndex, range.Length), 
                start,
                segmentSize,
                densitiesBuf,
                finalize,
                out Range processedSpanRange
            );

            processedRange = range.FirstBucketIndex == 0
                ? processedSpanRange
                : processedSpanRange.AddOffset(range.FirstBucketIndex);

            return densitiesBuf;
        }


        private static void CalculateDensities(
            EventEnumerable events, 
            ReadOnlySpan<Bucket> buckets, 
            long? start,
            long segmentSize, 
            Span<double> targetBuffer,
            bool finalize,
            out Range processedRange
        )
        {
            
            long filled = 0, unfilled = 0;
            long distance = segmentSize;
            long lastEventTime = -1, currentEventTime = 0;
            int segmentIndex = 0;
            int eventsCount = 0;
            EventType lastEventType = EventType.Stop;
            Bucket currentBucket;
            BucketEvent firstEvent = default, 
                        lastEvent = default, 
                        lastAccountedEvent = default;


            

            foreach (ref readonly var ev in events)
            {
                currentBucket = buckets[ev.BucketIndex];
                if (eventsCount == 0)
                {
                    firstEvent = ev;

                    // If the first event in the range does not mean the start time by itself
                    if (start.HasValue)
                    {
                        if (start > currentBucket.GetAbsoluteTimeForEvent(firstEvent.Event))
                            throw new ArgumentException("Start time must be not greater than the first event time in the range", nameof(start));

                        // We can consider event time passed through start parameter as the last event
                        lastEventTime = start.Value;
                        lastEventType = (EventType)((byte)~firstEvent.Event.EventType & 1);
                    }
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
                        targetBuffer[segmentIndex] = CalculateDensity(ref filled, ref unfilled, lastEventType, ref distance, segmentSize);
                        lastEventTime += oldDistance - distance;
                        distance = currentEventTime - lastEventTime;
                        segmentIndex++;
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

            
            if (finalize && segmentIndex < targetBuffer.Length)
            {
                // we processed the whole range but the last density value is not completely calculated
                distance = segmentSize;
                targetBuffer[segmentIndex] = CalculateDensity(ref filled, ref unfilled, lastEventType, ref distance, segmentSize);
                processedRange = new Range(firstEvent.BucketIndex, lastEvent.BucketIndex, firstEvent.EventIndex, lastEvent.EventIndex);
            }
            else if (!finalize)
            {
                // there are some other events to wait for
                processedRange = new Range(firstEvent.BucketIndex, lastAccountedEvent.BucketIndex, firstEvent.EventIndex, lastAccountedEvent.EventIndex);
            }
            else if (filled + unfilled == 0)
            {
                // we processed the whole range
                processedRange = new Range(firstEvent.BucketIndex, lastEvent.BucketIndex, firstEvent.EventIndex, lastEvent.EventIndex);
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
