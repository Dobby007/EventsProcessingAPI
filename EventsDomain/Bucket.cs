using System;

namespace EventsDomain
{
    public class Bucket
    {
        public const long MaxRelativeEventTime = 60000;

        public readonly long Offset;
        public readonly Event[] Events;
        public readonly Payload[] Payloads;
        public double Density;
        public bool NoPayloadsLoaded => Payloads == null;

        public Event GetFirstEvent()
        {
            if (Events.Length < 1)
                throw new InvalidOperationException();

            return Events[0];
        }

        public long GetAbsoluteTimeForEvent(in Event ev)
        {
            return Bucket.MaxRelativeEventTime * Offset + ev.RelativeTime;
        }

        public long GetAbsoluteTimeForEvent(int index)
        {
            return Bucket.MaxRelativeEventTime * Offset + Events[index].RelativeTime;
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
