using EventsProcessingAPI.Enumeration;
using System;
using System.Linq;
using UnitTests.Common;
using UnitTests.Fixtures;
using Xunit;

namespace UnitTests.Tests
{
    public class EnumerableTests : IClassFixture<SampleEventsFixture>
    {
        SampleEventsFixture _fixture;

        public EnumerableTests(SampleEventsFixture fixture)
        {
            _fixture = fixture;
        }


        [Theory]
        [InlineData(0, 38)]
        [InlineData(1, 6)]
        [InlineData(2, 24)]
        public void EnumerableReturnsCorrectNumberOfEvents(int index, int expectedEventsCount)
        {
            var container = _fixture.GetByIndex(index);
            var events = new EventEnumerable(container.Buckets, 0, container.GetLastBucket().GetLastEventIndex());

            int eventsCount = 0;
            foreach (var ev in events)
                eventsCount++;

            Assert.Equal(expectedEventsCount, eventsCount);
            Assert.Equal(expectedEventsCount, events.Count);
        }

        [Theory]
        [InlineData(44 * Durations.Second, 45 * Durations.Second, 0)]
        [InlineData(30 * Durations.Second + 119, 30 * Durations.Second + 129, 1)]
        [InlineData(30 * Durations.Second + 119, 30 * Durations.Second + 130, 1)]
        [InlineData(30 * Durations.Second + 119, 30 * Durations.Second + 131, 2)]
        [InlineData(338, 339, 0)]
        [InlineData(345, 350, 1)]
        [InlineData(1, 10, 1)]
        [InlineData(1, 13, 1)]
        [InlineData(0, 13, 2)]
        [InlineData(65 * Durations.Second, 66 * Durations.Second, 0)]
        public void ContainerReturnsExpectedNumberOfEvents(long start, long end, int expectedCount)
        {
            var container = _fixture.SampleEvents1;

            var events = container.GetEvents(start, end);
            
            int eventsCount = 0;
            foreach (var ev in events)
                eventsCount++;

            Assert.Equal(expectedCount, eventsCount);
            Assert.Equal(expectedCount, events.Count);



            var realEvents = container.GetRealEvents(start, end);

            eventsCount = 0;
            foreach (var ev in realEvents)
                eventsCount++;

            Assert.Equal(expectedCount, eventsCount);
            Assert.Equal(expectedCount, realEvents.Count);

        }

        [Theory]
        [InlineData(44 * Durations.Second, 45 * Durations.Second, 0)]
        [InlineData(30 * Durations.Second + 119, 30 * Durations.Second + 129, 1)]
        [InlineData(30 * Durations.Second + 119, 30 * Durations.Second + 130, 1)]
        [InlineData(30 * Durations.Second + 119, 30 * Durations.Second + 131, 2)]
        [InlineData(338, 339, 0)]
        [InlineData(345, 350, 0)]
        [InlineData(1, 10, 0)]
        [InlineData(1, 13, 1)]
        [InlineData(0, 13, 2)]
        [InlineData(65 * Durations.Second, 66 * Durations.Second, 0)]
        public void ContainerDoesNotReturnEventsOutOfRange(long start, long end, int expectedCount)
        {
            var container = _fixture.SampleEvents1;

            var events = container.GetEvents(start, end, false);

            int eventsCount = 0;
            foreach (var ev in events)
                eventsCount++;

            Assert.Equal(expectedCount, eventsCount);
            Assert.Equal(expectedCount, events.Count);



            var realEvents = container.GetRealEvents(start, end, false);

            eventsCount = 0;
            foreach (var ev in realEvents)
                eventsCount++;

            Assert.Equal(expectedCount, eventsCount);
            Assert.Equal(expectedCount, realEvents.Count);

        }

    }
}
