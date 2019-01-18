using System;
using System.Runtime.CompilerServices;

namespace EventsDomain
{
    public class Bucket
    {
        /// <summary>
        /// Maximum event time that can be stored in the bucket (resolution: 1 CPU tick (1/10 of microsecond)
        /// </summary>
        public const long MaxBucketEventTime = 10_000_000;
        

        public readonly long Offset;
        public readonly Event[] Events;
        public readonly Payload[] Payloads;

        public bool NoPayloadsLoaded => Payloads == null;

        public long StartTime => MaxBucketEventTime * Offset;

        public Event GetFirstEvent()
        {
            if (Events.Length < 1)
                throw new InvalidOperationException();

            return Events[0];
        }

        /// <summary>
        /// Resolution: cpu tick
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetAbsoluteTimeForEvent(Event ev)
        {
            return MaxBucketEventTime * Offset + 
                (ev.EventTimeHigh << 4 | ev.EventTimeLow >> 4) * 10 + 
                (ev.EventTimeLow & Event.CpuTickMask);
        }

        /// <summary>
        /// Resolution: cpu tick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetAbsoluteTimeForEvent(int index)
        {
            return GetAbsoluteTimeForEvent(Events[index]);
        }

        public Event GetLastEvent()
        {
            if (Events.Length < 1)
                throw new InvalidOperationException();

            return Events[Events.Length - 1];
        }

        public int GetLastEventIndex()
        {
            if (Events.Length < 1)
                throw new InvalidOperationException();

            return Events.Length - 1;
        }

        public int Size => Events.Length;

        public Bucket(long index, Event[] events, Payload[] payloads = null)
        {
            if (events == null || events.Length < 1)
            {
                throw new ArgumentException("Bucket with no events is not allowed", nameof(events));
            }

            if (payloads != null && payloads.Length < 1)
            {
                throw new ArgumentException(
                    "Bucket with empty array of payloads is not allowed. If payloads aren't required pass null as a parameter.", 
                    nameof(payloads));
            }

            Offset = index;
            Events = events;
            Payloads = payloads;
        }

        private Bucket(long index)
        {
            Offset = index;
        }

        public static Bucket CreateFakeBucket(long index)
        {
            return new Bucket(index);
        }
    }
}
