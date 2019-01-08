using EventsDomain;
using EventsProcessingAPI.DataRead;
using Xunit;
using System;

namespace UnitTests.Tests
{
    public class BucketCreation
    {

        [Fact]
        public void BuilderDoesNotCreateBucketWithNoEvents()
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(0);

            Assert.Null(builder.Build(false));
            Assert.Null(builder.Build(true));
        }

        [Fact]
        public void BuilderThrowsExceptionWhenStartWasNotCalled()
        {
            var builder = new BucketBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.Build(false));
            Assert.Throws<InvalidOperationException>(() => builder.Build(true));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(100, 0)]
        [InlineData(1_000_000, 0)]
        [InlineData(10_000_000, 1)]
        [InlineData(10_000_001, 1)]
        public void BuilderCreatesBucketWithCorrectOffset(long eventTime, long expectedBucketOffset)
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(eventTime);

            builder.AddEvent(new RealEvent(EventType.Start, eventTime));

            var bucket = builder.Build(false, false);
            Assert.Equal(expectedBucketOffset, bucket.Offset);

            bucket = builder.Build(true, false);
            Assert.Equal(expectedBucketOffset, bucket.Offset);
        }

        [Fact]
        public void PayloadsAreNotLoaded()
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(0);

            builder.AddEvent(new RealEvent(EventType.Start, 0));

            var bucket = builder.Build(false);
            Assert.True(bucket.NoPayloadsLoaded);
        }

        [Fact]
        public void PayloadsAreLoaded()
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(0);

            builder.AddEventWithPayload(new RealEvent(EventType.Start, 0), new Payload(1, 1, 1, 1));

            var bucket = builder.Build(true);
            Assert.False(bucket.NoPayloadsLoaded);
        }

        [Fact]
        public void EventsAndPayloadsCountEqual()
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(0);

            builder.AddEventWithPayload(new RealEvent(EventType.Start, 0), new Payload(1, 1, 1, 1));
            builder.AddEventWithPayload(new RealEvent(EventType.Stop, 10), new Payload(2, 2, 2, 2));

            var bucket = builder.Build(true);
            Assert.False(bucket.NoPayloadsLoaded);
            Assert.Equal(bucket.Payloads.Length, bucket.Events.Length);
        }

        [Theory]
        [InlineData(0, 10_000_000)]
        [InlineData(9_999_999, 20_000_000)]
        public void EventDoesNotFitIntoCreatedBucket(long firstEventTime, long eventTime)
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(firstEventTime);
            Assert.False(builder.IsFitIntoCurrentBucket(eventTime));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 9_999_999)]
        [InlineData(10_000_000, 19_999_999)]
        [InlineData(10_000_000, 10_000_000)]
        public void EventFitsIntoCreatedBucket(long firstEventTime, long eventTime)
        {
            var builder = new BucketBuilder();
            builder.StartNewBucket(firstEventTime);
            Assert.True(builder.IsFitIntoCurrentBucket(eventTime));
        }
    }
}
