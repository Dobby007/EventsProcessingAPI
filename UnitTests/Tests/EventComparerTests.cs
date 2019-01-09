using EventsDomain;
using EventsProcessingAPI.Ranges;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.Tests
{
    public class EventComparerTests
    {
        private readonly EventComparer _eventComparer;

        public EventComparerTests()
        {
            _eventComparer = new EventComparer();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(16)]
        [InlineData(1000000)]
        [InlineData(9_999_999)]
        [InlineData(5120)]
        [InlineData(2560)]
        [InlineData(368630)]
        [InlineData(368631)]


        public void EventTimesAreEqual(uint eventTime)
        {
            int compareResult = _eventComparer.Compare(
                new Event(EventType.Start, eventTime),
                new Event(EventType.Start, eventTime));

            Assert.Equal(0, compareResult);

        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(160, 150)]
        [InlineData(165, 162)]
        [InlineData(165, 164)]
        [InlineData(2560, 1280)]
        [InlineData(2561, 2560)]
        [InlineData(9_999_999, 9_999_998)]
        [InlineData(1_000_000, 999_999)]
        [InlineData(65535_10, 65535_00)]
        [InlineData(368630, 368620)]
        [InlineData(1000, 999)]
        public void FirstEventTimeIsGraterThanTheSecond(uint event1, uint event2)
        {
            var ev1 = new Event(EventType.Start, event1);
            var ev2 = new Event(EventType.Start, event2);
            int compareResult = _eventComparer.Compare(ev1, ev2);

            Assert.Equal(1, compareResult);

        }


        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(150, 160)]
        [InlineData(162, 165)]
        [InlineData(164, 165)]
        [InlineData(1280, 2560)]
        [InlineData(2560, 2561)]
        [InlineData(9_999_998, 9_999_999)]
        [InlineData(999_999, 1_000_000)]
        [InlineData(65535_00, 65535_10)]
        [InlineData(368620, 368630)]
        [InlineData(999, 1000)]
        public void FirstEventTimeIsLessThanTheSecond(uint event1, uint event2)
        {
            var ev1 = new Event(EventType.Start, event1);
            var ev2 = new Event(EventType.Start, event2);
            int compareResult = _eventComparer.Compare(ev1, ev2);

            Assert.Equal(-1, compareResult);

        }
    }
}
