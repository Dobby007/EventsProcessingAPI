using EventsDomain;
using EventsProcessingAPI;
using EventsProcessingAPI.Common;
using EventsProcessingAPI.DataRead;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTests.Common;

namespace UnitTests.Fixtures
{
    public class SampleEventsFixture
    {
        private readonly List<BucketContainer> _eventSamples;

        public BucketContainer SampleEvents1 => GetByIndex(0);
        public BucketContainer SampleEvents2 => GetByIndex(1);
        public BucketContainer SampleEvents3 => GetByIndex(2);


        public SampleEventsFixture()
        {
            _eventSamples = new List<BucketContainer> {
                CreateSampleEventsSet(0),
                CreateSampleEventsSet2(0),
                CreateSampleEventsSet3(0)
            };
        }


        public BucketContainer GetByIndex(int index)
        {
            return _eventSamples[index];
        }

        private BucketContainer CreateSampleEventsSet(long offset)
        {
            var events = new[]
            {
                // 0 - 337
                new EventPair(offset + 0, 12, TimeUnit.CpuTick),
                new EventPair(20, 200, TimeUnit.CpuTick),
                new EventPair(20, 20, TimeUnit.CpuTick),
                new EventPair(55, 10, TimeUnit.CpuTick),

                // 340μs - 4043μs
                new EventPair(offset + Durations.Microsecond * 34, 12, TimeUnit.Microsecond, true),
                new EventPair(Durations.Microsecond, 10, TimeUnit.Microsecond),
                new EventPair(Durations.Millisecond, 10, TimeUnit.Microsecond),
                new EventPair(Durations.Millisecond * 3, 10, TimeUnit.Microsecond),

                
                // 2s - 2s4043μs
                new EventPair(offset + Durations.Second * 2, 12, TimeUnit.Microsecond, true),
                new EventPair(Durations.Microsecond, 10, TimeUnit.Microsecond),
                new EventPair(Durations.Millisecond, 10, TimeUnit.Microsecond),
                new EventPair(Durations.Millisecond * 3, 10, TimeUnit.Microsecond),

                // 30s - 30.99999999s
                new EventPair(offset + Durations.Second * 30, 12, TimeUnit.Microsecond, true),
                new EventPair(Durations.Microsecond, 10, TimeUnit.Microsecond),
                new EventPair(Durations.Millisecond, 10, TimeUnit.Microsecond),
                new EventPair(Durations.Millisecond * 3, 10, TimeUnit.Microsecond),
                new EventPair(offset + Durations.Second * 31 - 13, 12, TimeUnit.CpuTick, true),

                // 31s - 43s
                new EventPair(offset + Durations.Second * 31, 12, TimeUnit.Second, true),

                // 64s - 64s337
                new EventPair(offset + 64 * Durations.Second, 337, TimeUnit.CpuTick, true)
            };

            return CreateBucketContainer(events, offset);
        }

        private BucketContainer CreateSampleEventsSet2(long offset)
        {
            var events = new[]
            {
                // 0 - 20
                new EventPair(offset + 0, 7, TimeUnit.CpuTick),
                new EventPair(4, 3, TimeUnit.CpuTick),
                new EventPair(3, 3, TimeUnit.CpuTick),
            };

            return CreateBucketContainer(events, offset);
        }

        private BucketContainer CreateSampleEventsSet3(long offset)
        {
            var events = new[]
            {
                new EventPair(offset + 0, 5, TimeUnit.Second),
                new EventPair(7, 3, TimeUnit.Second),
                new EventPair(1, 4, TimeUnit.Second),
                new EventPair(offset + 13 * Durations.Second, 14, TimeUnit.Second, true),
                new EventPair(8 * Durations.Second, 14, TimeUnit.Second),
                new EventPair(1 * Durations.Second, 3, TimeUnit.Second),
                new EventPair(1 * Durations.Second, 4, TimeUnit.Second),
                new EventPair(2 * Durations.Second, 2, TimeUnit.Second),
                new EventPair(10 * Durations.Second, 10, TimeUnit.Second),
                new EventPair(5 * Durations.Second, 2, TimeUnit.Second),
                new EventPair(5 * Durations.Second, 2, TimeUnit.Second),
                new EventPair(1 * Durations.Second, 4, TimeUnit.Second)
            };

            return CreateBucketContainer(events, offset);
        }


        private BucketContainer CreateBucketContainer(EventPair[] eventPairs, long offset)
        {
            var buckets = new List<Bucket>();
            var builder = new BucketBuilder();
            long previousEventTime = offset;
            
            foreach (var pair in eventPairs)
            {
                long startEventTime = pair.IsAbsoluteOffset ? offset + pair.Offset : previousEventTime + pair.Offset;
                var startEvent = new RealEvent(EventType.Start, startEventTime);
                var stopEvent = new RealEvent(EventType.Stop, startEventTime + pair.Duration * pair.DurationTimeUnit.GetTimeUnitDuration());
                

                if (!builder.IsFitIntoCurrentBucket(startEvent.Ticks))
                {
                    if (builder.HasEvents)
                        buckets.Add(builder.Build(false));

                    builder.StartNewBucket(startEvent.Ticks);
                }

                builder.AddEvent(startEvent);
                
                if (!builder.IsFitIntoCurrentBucket(stopEvent.Ticks))
                {
                    buckets.Add(builder.Build(false));
                    builder.StartNewBucket(stopEvent.Ticks);
                }

                builder.AddEvent(stopEvent);
                previousEventTime = stopEvent.Ticks;
            }

            var bucket = builder.Build(false);
            if (bucket != null)
                buckets.Add(bucket);

            return new BucketContainer(buckets.ToArray(), 0);
        }
    }
}
