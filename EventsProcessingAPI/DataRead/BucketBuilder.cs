using EventsDomain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EventsProcessingAPI.DataRead
{
    internal partial class BucketBuilder : IBucketBuilder
    {
        private long _bucketOffset = -1;
        private int _bucketSize = 0;
        // Pre-allocated buffers for events and payloads
        private Event[] _eventsBuffer = new Event[Bucket.MaxBucketEventTime];
        private Payload[] _payloadsBuffer = new Payload[Bucket.MaxBucketEventTime];

        public bool HasEvents => _bucketSize > 0;

        public void StartNewBucket(long firstEventTime)
        {
            _bucketOffset = firstEventTime / Bucket.MaxBucketEventTime;
            _bucketSize = 0;
        }

        public bool IsFitIntoCurrentBucket(long eventTime)
        {
            return _bucketOffset >= 0 && eventTime / Bucket.MaxBucketEventTime == _bucketOffset;
        }

        public Bucket Build(bool withPayloads, bool reset = true)
        {
            if (_bucketOffset < 0)
                throw new InvalidOperationException("Bucket should initialized first");

            if (_bucketSize == 0)
                return null;
            
            long offset = _bucketOffset;
            var eventsInBucket = new Event[_bucketSize];
            Array.Copy(_eventsBuffer, eventsInBucket, _bucketSize);
            Payload[] payloadsInBucket = null;

            if (withPayloads)
            {
                payloadsInBucket = new Payload[_bucketSize];
                Array.Copy(_payloadsBuffer, payloadsInBucket, _bucketSize);
            }

            if (reset)
                Reset();

            return new Bucket(offset, eventsInBucket, payloadsInBucket);
        }

        public void AddEvent(in RealEvent ev)
        {
            uint bucketEventTime = checked((uint)(ev.Ticks % Bucket.MaxBucketEventTime));
            _eventsBuffer[_bucketSize] = new Event(ev.EventType, bucketEventTime);
            EnsureEventOrderIsCorrect();
            _bucketSize++;
        }

        public void AddEventWithPayload(in RealEvent ev, in Payload payload)
        {
            uint bucketEventTime = checked((uint)(ev.Ticks % Bucket.MaxBucketEventTime));
            _eventsBuffer[_bucketSize] = new Event(ev.EventType, bucketEventTime);
            _payloadsBuffer[_bucketSize] = payload;
            EnsureEventOrderIsCorrect();
            _bucketSize++;
        }

        private void Reset()
        {
            _bucketSize = 0;
            _bucketOffset = -1;
        }

        partial void EnsureEventOrderIsCorrect();

#if DEBUG
        partial void EnsureEventOrderIsCorrect()
        {
            var curEvent = _eventsBuffer[_bucketSize];
            Debug.Assert(
                _bucketSize < 1 ||
                curEvent.EventTimeHigh > _eventsBuffer[_bucketSize - 1].EventTimeHigh ||
                (curEvent.EventTimeHigh == _eventsBuffer[_bucketSize - 1].EventTimeHigh && curEvent.EventTimeLow > _eventsBuffer[_bucketSize - 1].EventTimeLow)
            );
        }
#endif
    }
}
