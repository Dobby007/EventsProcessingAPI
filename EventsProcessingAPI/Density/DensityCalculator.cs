using EventsDomain;
using EventsProcessingAPI.Enumeration;
using EventsProcessingAPI.Exceptions;
using EventsProcessingAPI.Ranges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventsProcessingAPI.Density
{
    internal static class DensityCalculator
    {
        
        internal static void CalculateDensities(
            Memory<Bucket> bucketsArray,
            in DensityCalculationRequest request,
            Span<double> targetBuffer,
            bool finalize,
            out long processedRange
        )
        {
            var buckets = bucketsArray.Span;
            var range = RangeSelector.GetRange(bucketsArray.Span, request.Start, request.End + 1);
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
                    lastEventTime = request.Start;
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

                    int calculatedDensities = 0;
                    while (distance >= request.SegmentSize)
                    {
                        long oldDistance = distance;
                        targetBuffer[segmentIndex++] = CalculateDensity(ref filled, ref unfilled, lastEventType, ref distance, request.SegmentSize);
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
                distance = request.SegmentSize;
                targetBuffer[segmentIndex++] = CalculateDensity(ref filled, ref unfilled, lastEventType, ref distance, request.SegmentSize);

                // we need to set densities for segments that we didn't processed due to the lack of the events
                if (lastEventType == EventType.Start)
                    for (; segmentIndex < targetBuffer.Length; segmentIndex++)
                        targetBuffer[segmentIndex] = 1;

                processedRange = request.End - request.Start;
            }
            else if (!finalize)
            {
                // there are some other events to wait for
                if (segmentIndex == 0)
                    processedRange = 0; // we have not calculated any densities
                else 
                    processedRange = segmentIndex * request.SegmentSize;  // we have calculated some densities
            }
            else if (filled + unfilled == 0)
            {
                // we processed the whole range and calculated all the densities
                processedRange = request.End - request.Start;
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
