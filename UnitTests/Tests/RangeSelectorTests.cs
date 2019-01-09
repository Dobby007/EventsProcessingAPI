using EventsDomain;
using EventsProcessingAPI.Ranges;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTests.Common;
using UnitTests.Fixtures;
using Xunit;

namespace UnitTests.Tests
{
    public class RangeSelectorTests : IClassFixture<SampleEventsFixture>
    {
        SampleEventsFixture _fixture;

        public RangeSelectorTests(SampleEventsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(0, 338)]
        [InlineData(340, 461)]
        [InlineData(31 * Durations.Second, 43 * Durations.Second  + 1)]
        [InlineData(0, 64 * Durations.Second + 338)]
        public void SelectorReturnsBoundaryEvents(long start, long end)
        {
            var buckets = _fixture.SampleEvents1.Buckets;
            Range range = RangeSelector.GetRange(buckets, start, end);
            Assert.True(range.IsFound);

            Bucket firstBucket = buckets[range.FirstBucketIndex];
            Assert.Equal(start, firstBucket.GetAbsoluteTimeForEvent(range.FirstEventIndex));

            Bucket lastBucket = buckets[range.LastBucketIndex];
            Assert.Equal(end - 1, lastBucket.GetAbsoluteTimeForEvent(range.LastEventIndex));
        }

        [Fact]
        public void RangeIsNotFoundWhenBucketsArrayIsEmpty()
        {
            Range range = RangeSelector.GetRange(Array.Empty<Bucket>(), 0, long.MaxValue);
            Assert.False(range.IsFound);
        }

        [Fact]
        public void SelectorThrowsWhenEndEqualsOrLessThanStart()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                RangeSelector.GetRange(_fixture.SampleEvents1.Buckets, 1, 1);
            });
            Assert.Throws<ArgumentException>(() =>
            {
                RangeSelector.GetRange(_fixture.SampleEvents1.Buckets, 2, 1);
            });
        }

        [Theory]
        [InlineData(44 * Durations.Second, 45 * Durations.Second, 43 * Durations.Second)]
        [InlineData(30 * Durations.Second + 121, 30 * Durations.Second + 129, 30 * Durations.Second + 120)]
        [InlineData(338, 339, 337)]
        [InlineData(345, 350, 340)]
        [InlineData(1, 10, 0)]
        [InlineData(65 * Durations.Second, 66 * Durations.Second, 64 * Durations.Second + 337)]
        public void SelectorReturnsCorrectNearestEvent(long start, long end, long expectedNearestEventTime)
        {
            var buckets = _fixture.SampleEvents1.Buckets;
            Range range = RangeSelector.GetRange(buckets, start, end);
            Assert.False(range.IsFound);
            Assert.True(range.IsNearestEventFound);

            Bucket bucket = buckets[range.NearestBucketIndex];
            Assert.Equal(expectedNearestEventTime, bucket.GetAbsoluteTimeForEvent(range.NearestEventIndex));
        }

    }
}
