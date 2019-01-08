using EventsProcessingAPI.Density;
using System;
using System.Linq;
using UnitTests.Common;
using UnitTests.Fixtures;
using Xunit;

namespace UnitTests.Tests
{
    public class DensityCalculationWithoutHints : IClassFixture<SampleEventsFixture>
    {
        SampleEventsFixture _fixture;

        public DensityCalculationWithoutHints(SampleEventsFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void AllDensitiesEqualOne()
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, 31 * Durations.Second, 35 * Durations.Second - 1, Durations.Second);
            Assert.NotEmpty(densities);
            Assert.All(densities, d => Assert.Equal(1, d));
        }

        [Fact]
        public void NoDensities()
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, 255 * Durations.Second, 260 * Durations.Second - 1, Durations.Second);
            Assert.Empty(densities);
        }

        [Fact]
        public void LastDensityIsZero()
        {
            var densities = DensityCalculator.GetDensities(
                _fixture.SampleEvents1, 
                31 * Durations.Second, 
                44 * Durations.Second,
                Durations.Second);

            Assert.NotEmpty(densities);
            Assert.True(densities.Last() == 0);
        }

        [Theory]
        [InlineData(64 * Durations.Second, 65 * Durations.Second + 1, 500, 0.674)]
        [InlineData(64 * Durations.Second, 65 * Durations.Second + 1, Durations.Millisecond, 0.0337)]
        [InlineData(64 * Durations.Second, 65 * Durations.Second + 1, Durations.Second, 0.0000337)]
        [InlineData(35 * Durations.Second, 65 * Durations.Second + 1, Durations.Second * 30, 0.26666779)]
        [InlineData(35 * Durations.Second, 35 * Durations.Second + Durations.Minute + 1, Durations.Minute, 0.133333895)]

        public void CalculateDensityForOneSegment(long start, long end, long segmentSize, double expectedDensity)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, start, end, segmentSize);

            Assert.Single(densities);
            Assert.DensitiesEqual(expectedDensity, densities[0]);
        }


        [Theory]
        [InlineData(0, 10, 10, 1)]
        [InlineData(5, 10, 5, 1)]
        [InlineData(Durations.Microsecond * 34, Durations.Microsecond * 34 + 1, Durations.CpuTick, 1)]
        [InlineData(64 * Durations.Second + 337, 64 * Durations.Second + 338, Durations.CpuTick, 0)]
        [InlineData(Durations.Second * 31, Durations.Second * 32, Durations.Second, 1)]
        public void CalculateDensityForRangeWithOneStartEvent(long start, long end, long segmentSize, double expectedDensity)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, start, end, segmentSize);

            Assert.Single(densities);
            Assert.DensitiesEqual(expectedDensity, densities[0]);
        }

        [Theory]
        [InlineData(9, 19, 10, 0.3)]
        [InlineData(210, 220, 10, 1)]
        public void CalculateDensityForRangeWithOneStopEvent(long start, long end, long segmentSize, double expectedDensity)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, start, end, segmentSize);

            Assert.Single(densities);
            Assert.DensitiesEqual(expectedDensity, densities[0]);
        }

        [Theory]
        [InlineData(5, 10, 5, 1)]
        [InlineData(310, 320, 10, 0)]
        public void CalculateDensityForRangeWithNoEvents(long start, long end, long segmentSize, double expectedDensity)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, start, end, segmentSize);

            Assert.Single(densities);
            Assert.DensitiesEqual(expectedDensity, densities[0]);
        }

        [Theory]
        [InlineData(0, 12, 12, 1)]
        [InlineData(32, 232, 200, 1)]
        [InlineData(Durations.Second * 31, Durations.Second * 43, Durations.Second, 1)]
        public void CalculateDensityForRangeWithStartAndStopEvents(long start, long end, long segmentSize, double expectedDensity)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, start, end, segmentSize);
            Assert.All(densities, d => Assert.DensitiesEqual(expectedDensity, d));
        }

        [Theory]
        [InlineData(31 * Durations.Second, 44 * Durations.Second, Durations.Second, 13)]
        [InlineData(31 * Durations.Second, 32 * Durations.Second, Durations.Second, 1)]
        [InlineData(24 * Durations.Second, 25 * Durations.Second, Durations.Second, 1)]
        [InlineData(24 * Durations.Second, 24 * Durations.Second + 1, Durations.CpuTick, 1)]
        [InlineData(Durations.Second * 31, Durations.Second * 43, Durations.Second, 12)]
        [InlineData(0, 12, 12, 1)]
        [InlineData(20, 220, 200, 1)]
        public void DensitiesCountShouldMatch(long start, long end, long segmentSize, int expectedDensitiesCount)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents1, start, end, segmentSize);
            Assert.Equal(densities.Length, expectedDensitiesCount);
        }

        [Theory]
        [InlineData(0, 21, Durations.CpuTick * 5, new[] { 1D, 0.4, 0.6, 0.6 })]
        public void CalculateDensitiesForSeveralSegments(long start, long end, long segmentSize, double[] expectedDensities)
        {
            var densities = DensityCalculator.GetDensities(_fixture.SampleEvents2, start, end, segmentSize);
            
            Assert.Equal(densities.Length, expectedDensities.Length);
            for(var i = 0; i < densities.Length; i++)
            {
                Assert.Equal(expectedDensities[i], densities[i]);
            }
        }
    }
}
