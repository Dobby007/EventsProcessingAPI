using EventsProcessingAPI.Density;
using System;
using UnitTests.Common;
using UnitTests.Fixtures;
using Xunit;

namespace UnitTests.Tests
{
    public class ExceptionCases : IClassFixture<SampleEventsFixture>
    {
        SampleEventsFixture _fixture;

        public ExceptionCases(SampleEventsFixture fixture)
        {
            _fixture = fixture;
        }


        [Theory]
        [InlineData(0 * Durations.Second, 65 * Durations.Second, 500)]
        [InlineData(0, ((long)uint.MaxValue + 1) * 500, 500)]

        public void ShouldThrowExceptionWhenTooMuchSegments(long start, long end, long segmentSize)
        {
            Assert.Throws<ArgumentException>(() => {
                DensityCalculator.GetDensities(
                    _fixture.SampleEvents1,
                    start,
                    end,
                    segmentSize);
            });
        }

        [Fact]
        public void ShouldThrowExceptionWhenEndIsLessThanStart()
        {
            Assert.Throws<ArgumentException>(() => {
                DensityCalculator.GetDensities(
                    _fixture.SampleEvents1,
                    24 * Durations.Second,
                    24 * Durations.Second - 1,
                    Durations.Second);
            });
        }

        [Fact]
        public void ShouldThrowExceptionWhenEndEqualsStart()
        {
            Assert.Throws<ArgumentException>(() => {
                DensityCalculator.GetDensities(
                    _fixture.SampleEvents1,
                    24 * Durations.Second,
                    24 * Durations.Second,
                    Durations.Second);
            });
        }

        [Fact]
        public void ShouldThrowWhenSegmentSizeIsTooBig()
        {
            Assert.Throws<ArgumentException>(() => {
                DensityCalculator.GetDensities(
                    _fixture.SampleEvents1,
                    24 * Durations.Second,
                    24 * Durations.Second + 1000,
                    Durations.Second);
            });
        }

    }
}
