using EventsDomain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EventsProcessingAPI.Ranges
{
    public static class RangeSelector
    {
        public static Range GetRange(BucketContainer container, long start, long end)
        {
            return GetRange(container.Buckets, start, end, container.FirstTimestamp);
        }

        internal static Range GetRange(ReadOnlySpan<Bucket> buckets, long start, long end, long firstTimeStamp)
        {
            if (buckets.Length < 1)
            {
                return new Range(false);
            }

            if (start < firstTimeStamp)
                throw new ArgumentException(nameof(start));

            if (end < firstTimeStamp || end < start)
                throw new ArgumentException(nameof(end));

            try
            {
                var lowerBound = GetBound(buckets, start, false);
                var upperBound = GetBound(buckets, end, true);

                if (upperBound.bucketIndex < lowerBound.bucketIndex)
                {
                    return new Range(false);
                }

                return new Range(lowerBound.bucketIndex, upperBound.bucketIndex, lowerBound.eventIndex, upperBound.eventIndex);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new Range(false);
            }

        }
        

        private static (int bucketIndex, int eventIndex) GetBound(ReadOnlySpan<Bucket> buckets, long eventTime, bool isUpperBound = true)
        {
            var bucketOffset = eventTime / Bucket.MaxRelativeEventTime;
            var eventRelativeTime = eventTime % Bucket.MaxRelativeEventTime;

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
        private static int GetNearestBucketIndexForOffset(ReadOnlySpan<Bucket> buckets, long offset)
        {
            return buckets.BinarySearch(
                Bucket.CreateFakeBucket(offset / Bucket.MaxRelativeEventTime), 
                new BucketOffsetComparer()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNearestEventIndexForTime(Event[] events, long absTime)
        {
            return Array.BinarySearch(
                events,
                new Event(EventType.Start, (ushort)(absTime % Bucket.MaxRelativeEventTime)), 
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

        public readonly struct Range
        {
            public readonly int FirstBucketIndex;
            public readonly int LastBucketIndex;
            public readonly int FirstEventIndex;
            public readonly int LastEventIndex;

            /// <summary>
            /// Length of range, i.e. count of buckets included into the range.
            /// </summary>
            public int Length => LastBucketIndex - FirstBucketIndex + 1;

            public readonly bool IsFound;

            public Range(int firstBucketIndex, int lastBucketIndex, int firstEventIndex, int lastEventIndex) : this()
            {
                FirstBucketIndex = firstBucketIndex;
                LastBucketIndex = lastBucketIndex;
                FirstEventIndex = firstEventIndex;
                LastEventIndex = lastEventIndex;
                IsFound = true;
            }

            public Range(bool isFound) : this(0, 0, 0, 0)
            {
                IsFound = isFound;
            }

            public Range AddOffset(int offset)
            {
                return new Range(FirstBucketIndex + offset, LastBucketIndex + offset, FirstEventIndex, LastEventIndex);
            }
        }
    }
}
