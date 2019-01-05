using EventsDomain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EventsProcessingAPI.Ranges
{
    public static class RangeSelector
    {
        public static Range GetRange(in ReadOnlySpan<Bucket> buckets, in RangeRequest rangeRequest)
        {
            return GetRange(buckets, rangeRequest.Start, rangeRequest.End, rangeRequest.FirstTimeStamp);
        }

        public static Range GetRange(in ReadOnlySpan<Bucket> buckets, long start, long end, long firstTimeStamp)
        {
            if (buckets.Length < 1)
            {
                return new Range(false);
            }

            if (start < firstTimeStamp)
                start = firstTimeStamp;

            if (end < firstTimeStamp || end < start)
                throw new ArgumentException(nameof(end));

            try
            {
                var lowerBound = GetBound(buckets, start, false);
                var upperBound = GetBound(buckets, end, true);

                if (
                    upperBound.bucketIndex > lowerBound.bucketIndex ||
                    (upperBound.bucketIndex == lowerBound.bucketIndex && upperBound.eventIndex >= lowerBound.eventIndex)
                ) {
                    return new Range(lowerBound.bucketIndex, upperBound.bucketIndex, lowerBound.eventIndex, upperBound.eventIndex);
                }
                
                return new Range(false, upperBound.bucketIndex, upperBound.eventIndex);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new Range(false);
            }

        }
        

        private static (int bucketIndex, int eventIndex) GetBound(ReadOnlySpan<Bucket> buckets, long eventTime, bool isUpperBound = true)
        {
            int eventIndex = 0;
            int existingBucketIndex = GetNearestBucketIndexForOffset(buckets, eventTime);
            if (existingBucketIndex < 0)
            {
                existingBucketIndex = NormalizeIndex(
                    existingBucketIndex, 
                    preferLeftNeighbour: isUpperBound);
                eventIndex = isUpperBound ? buckets[existingBucketIndex].GetLastEventIndex() : 0;
            }
            else
            {
                eventIndex = GetNearestEventIndexForTime(buckets[existingBucketIndex].Events, eventTime);
                eventIndex = NormalizeIndex(eventIndex, preferLeftNeighbour: isUpperBound);
                if (eventIndex >= buckets[existingBucketIndex].Size)
                {
                    if (existingBucketIndex + 1 >= buckets.Length)
                        throw new ArgumentOutOfRangeException(nameof(eventTime));
                    else
                    {
                        existingBucketIndex++;
                        eventIndex = 0;
                    }

                }
                else  if (eventIndex < 0)
                {
                    if (existingBucketIndex - 1 < 0)
                        throw new ArgumentOutOfRangeException(nameof(eventTime));
                    else
                    {
                        existingBucketIndex--;
                        eventIndex = buckets[existingBucketIndex].GetLastEventIndex();
                    }
                }

            }

            return (existingBucketIndex, eventIndex);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNearestBucketIndexForOffset(ReadOnlySpan<Bucket> buckets, long absTime)
        {
            return buckets.BinarySearch(
                Bucket.CreateFakeBucket(absTime / Bucket.MaxBucketEventTime), 
                new BucketOffsetComparer()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNearestEventIndexForTime(Event[] events, long absTime)
        {
            return Array.BinarySearch(
                events,
                new Event(EventType.Start, (uint)(absTime % Bucket.MaxBucketEventTime)), 
                new EventComparer()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int NormalizeIndex(int index, bool preferLeftNeighbour)
        {
            if (index < 0 && preferLeftNeighbour)
            {
                index = ~index - 1;
            }
            else if (index < 0)
            {
                index = ~index;
            }

            return index;

        }

        
    }
}
